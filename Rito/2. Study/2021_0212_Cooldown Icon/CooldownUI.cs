using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-02-12 PM 5:44:41
// 작성자 : Rito

namespace Rito.CooldownIcon
{
    public class CooldownUI : MonoBehaviour
    {
        public Image fill;
        private float maxCooldown = 5f;
        private float currentCooldown = 5f;

        public void SetMaxCooldown(in float value)
        {
            maxCooldown = value;
            UpdateFiilAmount();
        }

        public void SetCurrentCooldown(in float value)
        {
            currentCooldown = value;
            UpdateFiilAmount();
        }

        private void UpdateFiilAmount()
        {
            fill.fillAmount = currentCooldown / maxCooldown;
        }

        // Test
        private void Update()
        {
            SetCurrentCooldown(currentCooldown - Time.deltaTime);

            // Loop
            if (currentCooldown < 0f)
                currentCooldown = maxCooldown;
        }
    }
}