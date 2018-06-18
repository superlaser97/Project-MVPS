﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SealTeam4
{
    /// <summary>
    /// 
    /// Author: Clement Leow, SealTeam4
    /// Project: St Michael
    /// 
    /// This script handles the UI interactions for the Game Master Interface
    /// </summary>
    public class InterfaceManager : MonoBehaviour
    {
        #region Variables

        #region Calibration Button variables
        private bool calibrationModeOn;
        private Image calibrationBtnColor;
        #endregion

        #region Selected Game Object Panel variables
        private GameObject selectedObject;
        private Text selectedObjectName;
        private RaycastHit hit;
        private Ray ray;
        private Shader outlineShader;
        private Renderer rend;
        #endregion

        #region Player container list spawning variables
        private int count;
        [SerializeField] private GameObject prefab;
        [SerializeField] private GameObject playerListUI;
        private List<GameObject> playerContainerList = new List<GameObject>();
        [SerializeField] private RectTransform rt;
        #endregion

        #region Action List spawning variables
        private List<UnityEngine.Object> actionList;
        #endregion

        #endregion

        // Use this for initialization
        void Start()
        {
            // Calibration Button Setup
            calibrationModeOn = false;
            calibrationBtnColor = GameObject.Find("PlayerPosCalibrationBtn").GetComponent<Image>();
            calibrationBtnColor.color = Color.grey;

            // Selection Setup
            selectedObjectName = GameObject.Find("Selected Object Name").GetComponent<Text>();
            outlineShader = Shader.Find("Outlined/Uniform");
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                try
                {
                    if (selectedObject != null)
                    {
                        selectedObject.GetComponent<Renderer>().material.shader = Shader.Find("Standard");
                    }
                    SelectGameObject();
                    selectedObjectName.text = selectedObject.name;
                    rend = selectedObject.GetComponent<Renderer>();
                    rend.material.shader = outlineShader;
                    rend.material.SetColor("_OutlineColor", Color.yellow);
                    rend.material.SetFloat("_OutlineWidth", 0.1f);
                }
                catch (Exception e)
                {
                    Debug.Log("Nothing Selected");
                }
            }
        }

        // Toggles the position Buttons on and off
        public void ToggleCalibration()
        {
            if (!calibrationModeOn)
            {
                calibrationModeOn = true;
                calibrationBtnColor.color = Color.cyan;

            }
            else
            {
                calibrationModeOn = false;
                calibrationBtnColor.color = Color.grey;
            }

            foreach (GameObject playerContainer in playerContainerList)
            {
                playerContainer.GetComponent<playerUIContainer>().SetButtonStates(calibrationModeOn);
            }
        }

        // Gets an object and puts it in focus
        private void SelectGameObject()
        {
            try
            {
                // Raycasts to find object for selection
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out hit);
                selectedObject = hit.transform.gameObject;

                // Future code don't delete
                // Gets action list from selected object
                // selectedObject.GetComponent<AIController>().GetActions();

            }
            catch (Exception e)
            {
                // Nothing here
            }

        }

        // Adds a new ui prefab to the player list
        public void AddNewPlayer()
        {
            // increases the viewport to account for the new player stats
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + 60);

            // Instantiate a new player UI prefab
            GameObject go = Instantiate(
                prefab,
                playerListUI.transform.position,
                Quaternion.identity,
                playerListUI.transform);

            // Checks if calibration mode is on, then sets visibility accordingly
            go.GetComponent<playerUIContainer>().SetButtonStates(calibrationModeOn);
            playerContainerList.Add(go);
        }

        // Removes a specific ui prefab to the player list
        public void RemoveExistingPlayer()
        {
            // decreases the viewport to account for removal of one row of stats
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y - 60);
        }
    }

}