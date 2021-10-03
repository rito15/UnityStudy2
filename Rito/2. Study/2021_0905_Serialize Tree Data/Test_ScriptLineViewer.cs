#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEditor;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Linq;

// 날짜 : 2021-09-04 PM 5:01:03
// 작성자 : Rito

namespace Rito
{
    /// <summary> 지정한 폴더 경로 내 모든 C# 스크립트 개수, 라인 수 집계 </summary>
    public class Test_ScriptLineViewer : MonoBehaviour
    {
        /***********************************************************************
        *                               Const Fields
        ***********************************************************************/
        #region .
        public const int TRUE = 1;
        public const int FALSE = 0;

        #endregion
        /***********************************************************************
        *                               GUI Methods
        ***********************************************************************/
        #region .
        [CustomEditor(typeof(Test_ScriptLineViewer))]
        private class CE : UnityEditor.Editor
        {
            Test_ScriptLineViewer m;

            [SerializeField]
            private DefaultAsset folderAsset;
            private GUIContent folderLabel;

            [SerializeField]
            private DirectoryTree treeRoot;

            [SerializeField]
            private Vector2 scrollPos = Vector2.zero;

            [SerializeField]
            private int isCalculating = FALSE;

            private int calculatingIndex = 0;
            private static readonly string[] calculatingStrings = new string[]
            {
                "Calculating",
                "Calculating.",
                "Calculating..",
                "Calculating...",
            };

            private readonly ConcurrentQueue<Action> onGuiChangedQueue = new ConcurrentQueue<Action>();

            private void OnEnable()
            {
                m = target as Test_ScriptLineViewer;
            }

            public override void OnInspectorGUI()
            {
                InitGUI();

                folderAsset = EditorGUILayout.ObjectField(folderLabel, folderAsset, typeof(DefaultAsset), false) as DefaultAsset;

                if (GUILayout.Button("Calculate"))
                {
                    DirectoryInfo di = (folderAsset == null) ?
                        new DirectoryInfo(Application.dataPath) :
                        folderAsset.GetDirectoryInfo();

                    Task.Run(() => CreateTreeData(di));
                }

                // 스크롤바 생성
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                if (isCalculating == TRUE)
                {
                    var oldAlign = EditorStyles.helpBox.alignment;
                    var oldSize = EditorStyles.helpBox.fontSize;
                    EditorStyles.helpBox.alignment = TextAnchor.MiddleLeft;
                    EditorStyles.helpBox.fontSize = 14;

                    int index = (calculatingIndex / 100) % calculatingStrings.Length;

                    EditorGUILayout.HelpBox(calculatingStrings[index], MessageType.Info);

                    calculatingIndex++;
                    Repaint();

                    EditorStyles.helpBox.alignment = oldAlign;
                    EditorStyles.helpBox.fontSize = oldSize;
                }
                else if (treeRoot != null)
                {
                    DrawTree();
                }

                EditorGUILayout.EndScrollView();

                // GUI 변경 사항 적용 처리
                if (Event.current.type != EventType.Layout && onGuiChangedQueue.IsEmpty == false)
                {
                    bool deq = onGuiChangedQueue.TryDequeue(out Action action);
                    if (deq) action();
                }
            }
            private void InitGUI()
            {
                if (folderLabel == null)
                    folderLabel = new GUIContent("Folder");
            }

            private void CreateTreeData(DirectoryInfo rootFolder)
            {
                // 기존에 작업 중인 경우에는 시도조차 X
                if (isCalculating == TRUE) return;

                // 작업 시작 예약
                onGuiChangedQueue.Enqueue(() =>
                {
                    calculatingIndex = 0;
                    isCalculating = TRUE;
                    Repaint();
                });

                var newTreeRoot = new DirectoryTree(rootFolder, 0); // 실제 처리

                // 변경 사항 적용 예약
                onGuiChangedQueue.Enqueue(() =>
                {
                    treeRoot = newTreeRoot;
                    isCalculating = FALSE;
                });
            }

            private void DrawTree()
            {
                Local_DrawTree(treeRoot);
                EditorGUI.indentLevel = 0;

                // 재귀적으로 트리 그리기
                void Local_DrawTree(DirectoryTree tree)
                {
                    if (tree.totalFileCount == 0) return;

                    GUILayoutUtility.GetRect(0f, 4f); // Vertical Space
                    EditorGUI.indentLevel = tree.depth;

                    tree.foldout = EditorGUILayout.Foldout(tree.foldout, $"{tree.folderName} [C# Files : {tree.totalFileCount}, Total Lines : {tree.totalLineCount}]", true);
                    if (tree.foldout)
                    {
                        // Draw Folders
                        for (int i = 0; i < tree.FolderCount; i++)
                        {
                            Local_DrawTree(tree.folders[i]);
                        }

                        EditorGUI.indentLevel = tree.depth + 1;

                        // Draw File Labels
                        Color col = GUI.color;
                        GUI.color = Color.yellow * 2f;
                        for (int i = 0; i < tree.FileCount; i++)
                        {
                            EditorGUILayout.LabelField($"{tree.files[i].fileName} : {tree.files[i].lineCount}");
                        }
                        GUI.color = col;
                    }
                }
            }
        }

        
        #endregion
        /***********************************************************************
        *                               Class Definitions
        ***********************************************************************/
        #region .
        // 직렬화 시, 재귀 직렬 문제 발생 (원인 : DirectoryTree[] folders 필드)
        //[System.Serializable]
        public class DirectoryTree
        {
            //private DirectoryInfo folderInfo;
            public string absFolderPath;
            public string folderName;
            public int depth;
            public int totalFileCount; // 하위 파일 개수 합
            public int totalLineCount; // 하위 파일들의 라인 수 합

            public int FileCount => files?.Length ?? 0;
            public int FolderCount => folders?.Length ?? 0;

            public bool foldout = false;

            public DirectoryTree[] folders;
            public FileLineData[] files;

            public DirectoryTree(DirectoryInfo folderInfo, int depth, DirectoryTree parent = null)
            {
                //this.folderInfo = folderInfo;
                this.folderName = folderInfo.Name;
                this.depth = depth;
                this.totalLineCount = 0;

                InitFolders(folderInfo);
                InitCsFiles(folderInfo);

                // 라인 수 집계를 부모에 가산
                if (parent != null)
                {
                    parent.totalFileCount += this.totalFileCount;
                    parent.totalLineCount += this.totalLineCount;
                }
            }

            /// <summary> 하위 폴더 트리 생성 </summary>
            private void InitFolders(DirectoryInfo folderInfo)
            {
                DirectoryInfo[] subFolders = folderInfo.GetDirectories();
                int subFolderCount = subFolders.Length;

                if (subFolderCount > 0)
                {
                    folders = new DirectoryTree[subFolderCount];

                    for (int i = 0; i < subFolderCount; i++)
                    {
                        folders[i] = new DirectoryTree(subFolders[i], this.depth + 1, this);
                    }
                }
            }

            /// <summary> 하위 C# 파일 목록 생성 </summary>
            private void InitCsFiles(DirectoryInfo folderInfo)
            {
                FileInfo[] csFiles = folderInfo.GetFiles("*.cs");
                int csFileCount = csFiles.Length;

                if (csFileCount > 0)
                {
                    files = new FileLineData[csFileCount];

                    for (int i = 0; i < csFileCount; i++)
                    {
                        int lineCount = File.ReadAllLines(csFiles[i].FullName).Length;
                        files[i] = new FileLineData(csFiles[i].Name, lineCount);
                        totalLineCount += lineCount;
                    }
                }

                totalFileCount += csFileCount;
            }
        }

        [System.Serializable]
        public struct FileLineData
        {
            public string fileName;
            public int lineCount;

            public FileLineData(string fileName, int lineCount)
            {
                this.fileName = fileName;
                this.lineCount = lineCount;
            }
        }
        #endregion
    }
}

#endif