using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

// 날짜 : 2021-09-05 PM 8:18:54
// 작성자 : Rito

namespace Rito.Tests
{
    /// <summary> 
    /// 트리 자료구조 직렬화 예제
    /// </summary>
    public class Test_TreeSerialization : MonoBehaviour
    {
        public Tree tree;

        private void Awake()
        {
            if (tree != null && tree.IsRestorationRequired())
            {
                tree.Restore();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                MakeSampleTree();
        }

        private void MakeSampleTree()
        {
            Debug.Log("Sample Tree Generated");

            TreeNode[] nodes = new TreeNode[15];
            for (int i = 0; i < nodes.Length; i++)
                nodes[i] = new TreeNode($"{i}");

            nodes[0].AddChild(nodes[1]);
            nodes[0].AddChild(nodes[5]);
            nodes[0].AddChild(nodes[8]);
            nodes[1].AddChild(nodes[2]);
            nodes[1].AddChild(nodes[3]);
            nodes[1].AddChild(nodes[4]);
            nodes[5].AddChild(nodes[6]);
            nodes[5].AddChild(nodes[7]);
            nodes[8].AddChild(nodes[9]);
            nodes[8].AddChild(nodes[13]);
            nodes[9].AddChild(nodes[10]);
            nodes[9].AddChild(nodes[12]);
            nodes[10].AddChild(nodes[11]);
            nodes[13].AddChild(nodes[14]);

            tree = new Tree(nodes[0]);
            tree.Save();
        }

        /***********************************************************************
        *                           Tree Node Definition
        ***********************************************************************/
        #region .
        public class TreeNode
        {
            public NodeData data;
            public List<TreeNode> children;

            public TreeNode(NodeData data)
            {
                this.children = new List<TreeNode>();
                this.data = data;
            }

            public TreeNode(string name)
            {
                this.children = new List<TreeNode>();
                this.data = new NodeData(name);
            }

            public void AddChild(TreeNode child)
            {
                children.Add(child);
            }

            public void PrintNode()
            {
                int depth = 0;
                const int MAX_DEPTH = 100;

                Local_PrintNode(this);

                void Local_PrintNode(TreeNode node)
                {
                    Debug.Log($"Data : {node.data.name}, ChildCount : {node.children.Count}");
                    if (++depth > MAX_DEPTH) return;

                    foreach (var child in node.children)
                    {
                        Local_PrintNode(child);
                    }
                }
            }
        }

        #endregion
        /***********************************************************************
        *                           Node Data Definition
        ***********************************************************************/
        #region .
        [System.Serializable]
        public class NodeData
        {
            public string name;

#if UNITY_EDITOR
            public bool foldout;
#endif

            public NodeData(string name)
            {
                this.name = name;
            }
        }

        #endregion
        /***********************************************************************
        *                           Serializable Tree Node Definition
        ***********************************************************************/
        #region .
        [System.Serializable]
        public class SerializableTreeNode
        {
            public NodeData data;
            public int childCount;

            public SerializableTreeNode(TreeNode node)
            {
                this.data = node.data;
                this.childCount = node.children.Count;
            }

            public TreeNode Deserialize()
            {
                return new TreeNode(data);
            }
        }

        [System.Serializable]
        public class SerializedTree
        {
            public List<SerializableTreeNode> nodeList;

            public SerializedTree()
            {
                nodeList = new List<SerializableTreeNode>();
            }

            public SerializedTree(TreeNode rootNode) : this()
            {
                SerializeFromTree(rootNode);
            }

            public bool HasData()
            {
                return nodeList.Count > 0;
            }

            public void SerializeFromTree(TreeNode source)
            {
                nodeList.Clear();
                Local_SerializeAll(source);

                // 재귀 : 지정한 노드와 그 하위 노드를 모두 직렬화
                void Local_SerializeAll(TreeNode current)
                {
                    // 직렬화용 노드 생성, 리스트에 추가
                    nodeList.Add(new SerializableTreeNode(current));

                    // 자식 순회
                    foreach (var child in current.children)
                    {
                        Local_SerializeAll(child);
                    }
                }
            }

            public TreeNode Deserialize()
            {
                if (nodeList.Count == 0) return null;

                int index = 0;
                TreeNode root = Local_DeserializeAll();

                return root;

                // 재귀 : 루트로부터 모든 자식들 역직렬화 및 트리 생성
                TreeNode Local_DeserializeAll()
                {
                    //Debug.Log($"Deserialize : {index}");

                    int currentIndex = index;
                    TreeNode current = nodeList[currentIndex].Deserialize();

                    index++;

                    // 자식이 있을 경우, 자식을 역직렬화해서 자식목록에 추가
                    // 그리고 다시 그 자식에서 재귀
                    for (int i = 0; i < nodeList[currentIndex].childCount; i++)
                    {
                        current.AddChild(Local_DeserializeAll());
                    }

                    return current;
                }
            }
        }

        #endregion
        /***********************************************************************
        *                           Tree Definition
        ***********************************************************************/
        #region .

        [System.Serializable]
        public class Tree
        {
            public TreeNode root;
            public SerializedTree serializedTree;

            public Tree()
            {
                this.serializedTree = new SerializedTree();
            }

            public Tree(TreeNode rootNode) : this()
            {
                this.root = rootNode;
            }

            /// <summary> 트리 복원 필요 여부 확인 </summary>
            public bool IsRestorationRequired()
            {
                return root == null && serializedTree.HasData();
            }

            /// <summary> 현재 트리의 노드들을 직렬 트리에 저장 </summary>
            public void Save()
            {
                if (root == null) return;
                serializedTree.SerializeFromTree(root);

                Debug.Log("Tree Saved");
            }

            /// <summary> 직렬 트리로부터 트리 복원 </summary>
            public void Restore()
            {
                if (!serializedTree.HasData()) return;
                this.root = serializedTree.Deserialize();

                Debug.Log("Tree Restored");
            }
        }

        #endregion
        /***********************************************************************
        *                           Custom Editor
        ***********************************************************************/
        #region .
#if UNITY_EDITOR
        [CustomEditor(typeof(Test_TreeSerialization))]
        private class CE : UnityEditor.Editor
        {
            private Test_TreeSerialization m;

            private void OnEnable()
            {
                m = target as Test_TreeSerialization;
            }

            public override void OnInspectorGUI()
            {
                // 변경사항을 인식하고 저장하기 위해 Undo 등록
                Undo.RecordObject(m, nameof(Test_TreeSerialization));

                if (GUILayout.Button("Reset Tree"))
                {
                    m.tree.root = null;
                    m.tree.serializedTree = null;
                }
                if (GUILayout.Button("Generate Sample Tree"))
                {
                    GenerateSampleTree();
                }

                BuildTree(m.tree);
                DrawTree(m.tree);
            }

            private void GenerateSampleTree()
            {
                m.MakeSampleTree();
            }

            /// <summary> 에디터 작업을 위해 원본 트리 복원 </summary>
            private void BuildTree(Tree tree)
            {
                if (tree == null) return;
                if (Event.current.type != EventType.Layout) return;

                if (tree.IsRestorationRequired())
                {
                    tree.Restore();
                    Debug.Log($"Build Tree From Serialized Tree Bucket - {Event.current.type}");
                }
            }

            /// <summary> 재귀적 Foldout 형태로 트리 그려주기 </summary>
            private void DrawTree(Tree tree)
            {
                if (tree == null) return;
                if (tree.root == null) return;

                Local_DrawTreeNode(tree.root, 0);

                void Local_DrawTreeNode(TreeNode node, int depth)
                {
                    EditorGUI.indentLevel = depth;
                    node.data.foldout = EditorGUILayout.Foldout(node.data.foldout, node.data.name);

                    if (node.data.foldout)
                    {
                        node.data.name = EditorGUILayout.TextField("Data", node.data.name);
                        foreach (var child in node.children)
                        {
                            Local_DrawTreeNode(child, depth + 1);
                        }
                    }
                }
            }
        }
#endif
        #endregion
    }
}