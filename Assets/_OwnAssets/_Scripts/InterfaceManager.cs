﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;

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
        public static InterfaceManager instance;

        [SerializeField] private GameObject camPrefab;
        [SerializeField] private Camera cam;

        // Calibration Button variables
        private bool calibrationModeOn;
        private Image calibrationBtnColor;

        [Header("Selected Game Object Panel variables")]
        [SerializeField] private GameObject currSelectedGO;
        [SerializeField] private TextMeshProUGUI selectedGOTxt;
        private RaycastHit hit;
        private Ray ray;
        private Shader outlineShader;
        private Renderer rend;

        [Header("Player container list spawning variables")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private GameObject playerList_GO;
        [SerializeField] private RectTransform rt;
        private int count;
        private List<GameObject> playerContainerList = new List<GameObject>();

        [Header("Action List spawning variables")]
        [SerializeField] private Transform actionBtnContainer;
        [SerializeField] private GameObject actionBtn_Prefab;
        private List<string> actionList = new List<string>();
        private List<Button> actionBtnList = new List<Button>();

        [Header("Selection Marker Variable")]
        [SerializeField] GameObject markerPrefab;
        private GameObject marker;
        private bool markerActive;
        private Vector3 pos;
        private Vector3 screenPos;
        [SerializeField] private GameObject borderUiPrefab;
        private GameObject borderUI;
        private List<GameObject> markerList = new List<GameObject>();
        private bool inView;
        private Plane[] planes;
        private Collider col;

        [Header("MarkerUI Camera Properties")]
        private GameObject markerUICameraGO;
        private GameObject camToFollow;

        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button spawnNPCsButton;
        [SerializeField] private Button spawnAccessoriesButton;

        [Header("More Info Panel Properties")]
        [SerializeField] private GameObject MIP_Main;
        [SerializeField] private GameObject MIP_Label;
        [SerializeField] private TextMeshProUGUI MIP_title_1;
        [SerializeField] private TextMeshProUGUI MIP_title_1_sub;
        [SerializeField] private TextMeshProUGUI MIP_title_2;
        [SerializeField] private TextMeshProUGUI MIP_content;
        private IObjectInfo currSelObjInfo;
        private int currHighlightedContent = -1;

        private bool spawnedNPC = false;
        private bool spawnedAccessories = false;
        #endregion

        // Use this for initialization
        void Start()
        {
            if (!instance)
                instance = this;
            else
            {
                Debug.Log("Interface Manager already exists");
            }

            cam = Instantiate(camPrefab).GetComponentInChildren<Camera>();

            // Calibration Button Setup
            calibrationModeOn = false;
            calibrationBtnColor = GameObject.Find("PlayerPosCalibrationBtn").GetComponent<Image>();
            calibrationBtnColor.color = Color.grey;

            // Selection Setup
            outlineShader = Shader.Find("Outlined/Uniform");
            borderUI = Instantiate(borderUiPrefab, this.GetComponent<Canvas>().transform);
            borderUI.SetActive(false);

            GameObject.Find("AdminCam").transform.rotation = Quaternion.identity;

            // Setup MarkerUICamera
            GameObject markerUICamera = GameObject.Find("MarkerUICamera(Clone)");
            markerUICamera.transform.SetParent(cam.gameObject.transform);
            markerUICamera.transform.localPosition = new Vector3(0, 0, 0);
            markerUICamera.transform.rotation = Quaternion.identity;

            //Suscribe buttons to listerners
            startButton.onClick.AddListener(delegate { OnStartGameButtonClick(); });
            spawnNPCsButton.onClick.AddListener(delegate { OnSpawnNPCsButtonClick(); });
            spawnAccessoriesButton.onClick.AddListener(delegate { OnSpawnAccessoriesButtonClick(); });

        }

        // Update is called once per frame
        private void Update()
        {
            if (
                Input.GetKeyDown(KeyCode.Mouse0) && 
                (ReplaySystemCameraScript.instance.MouseActive) && 
                !EventSystem.current.IsPointerOverGameObject()
                )
            {
                TryToSelectGameObject();
            }


            if (currSelectedGO)
            {
                selectedGOTxt.text = currSelectedGO.GetComponent<IActions>().GetName();
                currSelObjInfo = currSelectedGO.GetComponent<IObjectInfo>();

                if (currSelObjInfo != null)
                {
                    MIP_Label.SetActive(true);
                    if (Input.GetKeyDown(KeyCode.I))
                        UpdateMoreInfoPanel(currSelObjInfo.GetObjectInfo());

                    if (Input.GetKey(KeyCode.I))
                    {
                        MIP_Main.SetActive(true);
                        if(currHighlightedContent != currSelObjInfo.GetContentIndexToHighlight())
                        {
                            currHighlightedContent = currSelObjInfo.GetContentIndexToHighlight();
                            UpdateMoreInfoPanel(currSelObjInfo.GetObjectInfo());
                        }
                    }
                    else
                    {
                        MIP_Main.SetActive(false);
                    }
                }
                else
                {
                    currSelObjInfo = null;
                    MIP_Label.SetActive(false);
                }
            }
            else
                selectedGOTxt.text = "Nothing Selected";

            UpdateMarker();
            UpdateActionList();
            ListenForKeys();
        }

        private void UpdateMoreInfoPanel(ObjectInfo objInfo)
        {
            MIP_title_1.text = "";
            MIP_title_1_sub.text = "";
            MIP_title_2.text = "";
            MIP_content.text = "";

            MIP_title_1.text = objInfo.title_1;
            MIP_title_1_sub.text = objInfo.title_1_sub;
            MIP_title_2.text = objInfo.title_2;

            for (int i = 0; i < objInfo.content.Count; i++)
            {
                if(i != currHighlightedContent)
                    MIP_content.text += objInfo.content[i] + "\n";
                else
                    MIP_content.text += "<color=red>" + objInfo.content[i] + "</color>\n";
            }
        }

        private void TryToSelectGameObject()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                // Raycasts to find object for selection
                ray = cam.ScreenPointToRay(Input.mousePosition);

                int ignoreLayer = ~(1 << LayerMask.NameToLayer("UI"));

                Physics.Raycast(ray, out hit, Mathf.Infinity, ignoreLayer);

                if (hit.transform)
                {
                    if(hit.transform.GetComponents<IActions>().Length != 0)
                    {
                        currSelectedGO = hit.transform.gameObject;
                        col = currSelectedGO.GetComponent<IActions>().GetCollider();
                        DrawObjectMarker();
                    }
                }
                else
                {
                    currSelectedGO = null;
                }
            }
        }

        public void SelectGameObject(GameObject go)
        {
            currSelectedGO = go;

            if(currSelectedGO)
            {
                col = currSelectedGO.GetComponent<IActions>().GetCollider();
                DrawObjectMarker();
            }
        }

        private void DrawObjectMarker()
        {
            pos = currSelectedGO.GetComponentInChildren<IActions>().GetHighestPointPos();
            screenPos = cam.WorldToScreenPoint(pos);
            if (currSelectedGO)
            {
                if (markerList.Count != 0)
                {
                    foreach (GameObject g in markerList)
                    {
                        Destroy(g.gameObject);
                    }
                    markerList.Clear();
                }
                borderUI.SetActive(false);
                GameObject m = Instantiate(markerPrefab, screenPos, Quaternion.identity, this.GetComponent<Canvas>().transform);
                markerList.Add(m);
            }
        }

        private void UpdateMarker()
        {
            if (markerList.Count == 0)
            {
                return;
            }

            if (currSelectedGO)
            {
                planes = GeometryUtility.CalculateFrustumPlanes(cam);
                inView = GeometryUtility.TestPlanesAABB(planes, col.bounds);

                if (!inView /*screenPos.z < 0*/)
                {
                    borderUI.SetActive(true);
                    markerList.First().SetActive(false);
                }
                else
                {
                    borderUI.SetActive(false);
                    markerList.First().gameObject.SetActive(true);
                    pos = currSelectedGO.GetComponentInChildren<IActions>().GetHighestPointPos();
                    screenPos = cam.WorldToScreenPoint(pos);
                    markerList.First().transform.position = screenPos;
                }
            }
            else
            {
                Destroy(markerList.First().gameObject);
                markerList.Clear();
            }
        }

        private void ListenForKeys()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && actionBtnList.Count > 0)
                actionBtnList[0].onClick.Invoke();

            if (Input.GetKeyDown(KeyCode.Alpha2) && actionBtnList.Count > 1)
                actionBtnList[1].onClick.Invoke();

            if (Input.GetKeyDown(KeyCode.Alpha3) && actionBtnList.Count > 2)
                actionBtnList[2].onClick.Invoke();

            if (Input.GetKeyDown(KeyCode.Alpha4) && actionBtnList.Count > 3)
                actionBtnList[3].onClick.Invoke();

            if (Input.GetKeyDown(KeyCode.Alpha5) && actionBtnList.Count > 4)
                actionBtnList[4].onClick.Invoke();
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

        private void UpdateActionList()
        {
            if (currSelectedGO)
            {
                if (currSelectedGO.GetComponents<IActions>().Length != 0)
                {
                    List<string> newActionList = currSelectedGO.GetComponent<IActions>().GetActions();
                    if (actionList.SequenceEqual(newActionList))
                    {
                        return;
                    }
                    else
                    {
                        actionList = new List<string>(newActionList);
                        UpdateActionListButtons();
                    }
                }
            }
            else
            {
                selectedGOTxt.text = "Nothing Selected";
                actionList.Clear();
                UpdateActionListButtons();
            }
        }

        public void UpdateActionListButtons()
        {
            foreach (Button btn in actionBtnList)
            {
                Destroy(btn.gameObject);
            }
            actionBtnList.Clear();

            if (actionList != null && actionList.Count > 0)
            {
                for (int i = 0; i < actionList.Count; i++)
                {
                    Button actionBtn = Instantiate(actionBtn_Prefab, actionBtnContainer).GetComponent<Button>();

                    actionBtn.GetComponentInChildren<TextMeshProUGUI>().text = (i + 1) + " - " + actionList[i];

                    actionBtn.onClick.AddListener(delegate
                    {
                        OnBtnClick(actionBtn);
                    });

                    actionBtnList.Add(actionBtn);
                }
            }
        }

        public void OnBtnClick(Button btn)
        {
            string text = btn.GetComponentInChildren<TextMeshProUGUI>().text.Substring(4);
            SendAction(text);
        }

        private void SendAction(string text)
        {
            currSelectedGO.GetComponent<IActions>().SetAction(text);
        }

        // Adds a new ui prefab to the player list
        public void AddNewPlayer()
        {
            // increases the viewport to account for the new player stats
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + 60);

            // Instantiate a new player UI prefab
            GameObject go = Instantiate(
                prefab,
                playerList_GO.transform.position,
                Quaternion.identity,
                playerList_GO.transform);

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

        private void OnStartGameButtonClick()
        {
            if (!spawnedNPC)
            {
                GameManager.instance.SpawnAndSetupNPC();
                Destroy(spawnNPCsButton.gameObject);
                spawnedNPC = true;
            }

            if (!spawnedAccessories)
            {
                GameManager.instance.SpawnAccessories();
                Destroy(spawnAccessoriesButton.gameObject);
                spawnedAccessories = true;
            }

            GameManager.instance.GM_Host_SwitchToRun();
            Destroy(startButton.gameObject);
        }

        private void OnSpawnNPCsButtonClick()
        {
            GameManager.instance.SpawnAndSetupNPC();
            Destroy(spawnNPCsButton.gameObject);

            spawnedNPC = true;
        }

        private void OnSpawnAccessoriesButtonClick()
        {
            GameManager.instance.SpawnAccessories();
            Destroy(spawnAccessoriesButton.gameObject);

            spawnedAccessories = true;
        }
    }

}