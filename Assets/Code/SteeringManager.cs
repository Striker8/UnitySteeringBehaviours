﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BGE.Scenarios;

namespace BGE
{
    

    public class SteeringManager : MonoBehaviour
    {
        List<Scenario> scenarios = new List<Scenario>();
        
        public Scenario currentScenario;
        StringBuilder message = new StringBuilder();
        
        public GameObject camFighter;

        public GameObject boidPrefab;
        public GameObject leaderPrefab;
        public Space space;
        static SteeringManager instance;
        // Use this for initialization
        GUIStyle style = new GUIStyle();

        bool camFollowing = false;

        GameObject monoCamera;
        GameObject activeCamera;
        GameObject riftCamera;             
        
        void Awake()
        {
            DontDestroyOnLoad(this);
        }

        void Start()
        {
            instance = this;
            Screen.showCursor = false;

            style.fontSize = 18;
            style.normal.textColor = Color.white;

            space = new Space();
            
            scenarios.Add(new SeekScenario());
            scenarios.Add(new ArriveScenario());
            scenarios.Add(new PursueScenario());
            scenarios.Add(new WanderScenario());
            scenarios.Add(new PathFollowingScenario());
            scenarios.Add(new ObstacleAvoidanceScenario());
            scenarios.Add(new FlockingScenario());
            scenarios.Add(new StateMachineScenario());
            scenarios.Add(new PathFindingScenario());
            currentScenario = scenarios[0];
            Params.cellSpacePartitioning = true;
            currentScenario.Start();

            monoCamera = GameObject.FindGameObjectWithTag("MainCamera");
            riftCamera = GameObject.FindGameObjectWithTag("ovrcamera");

            if (OVRDevice.SensorCount > 0)
            {
                riftCamera.SetActive(true);
                activeCamera = riftCamera;
            }
            else
            {
                riftCamera.SetActive(false);
                activeCamera = monoCamera;
            }

        }

        public static SteeringManager Instance
        {
            get
            {
                return instance;
            }
        }

        void OnGUI()
        {
            if (Params.showMessages)
            {
                GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "" + message, style);
            }
            if (Event.current.type == EventType.Repaint)
            {
                message.Length = 0;
            }

            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.F1)
                {
                    camFollowing = !camFollowing;
                }

                for (int i = 0; i < scenarios.Count; i++)
                {
                    if (Event.current.keyCode == KeyCode.Alpha0 + i)
                    {
                        currentScenario.TearDown();
                        currentScenario = scenarios[i];
                        currentScenario.Start();
                    }
                }

                float timeModRate = 0.1f;
                if (Event.current.keyCode == KeyCode.F2)
                {
                    Params.timeModifier += Time.deltaTime * timeModRate;
                }

                if (Event.current.keyCode == KeyCode.F3)
                {
                    Params.timeModifier -= Time.deltaTime * timeModRate;
                }

                if (Event.current.keyCode == KeyCode.F4)
                {
                    Params.showMessages = !Params.showMessages;
                }

                if (Event.current.keyCode == KeyCode.F5)
                {
                    Params.drawVectors = !Params.drawVectors;
                }
                
                if (Event.current.keyCode == KeyCode.F6)
                {
                    Params.drawDebugLines = !Params.drawDebugLines;
                }

                if (Event.current.keyCode == KeyCode.F7)
                {
                    activeCamera.transform.up = Vector3.up;
                }
                if (Event.current.keyCode == KeyCode.F8)
                {
                    Params.cellSpacePartitioning = !Params.cellSpacePartitioning;
                }
                if (Event.current.keyCode == KeyCode.F9)
                {
                    Params.enforceNonPenetrationConstraint = !Params.enforceNonPenetrationConstraint;
                }
                if (Event.current.keyCode == KeyCode.F10)
                {

                    Params.riftEnabled = !Params.riftEnabled;
                    if (Params.riftEnabled)
                    {
                        riftCamera.SetActive(true);
                        activeCamera = riftCamera;
                    }
                    else
                    {
                        riftCamera.SetActive(false);
                        activeCamera = riftCamera;
                    }
                }

                if (Event.current.keyCode == KeyCode.F11)
                {
                    if (Params.timeModifier != 0)
                    {
                        Params.timeModifier = 0.0f;
                    }
                    else
                    {
                        Params.timeModifier = 1.0f;
                    }
                }
                
                if (Event.current.keyCode == KeyCode.Escape)
                {
                    Application.Quit();
                }                
            }
        }

        public static void PrintMessage(string message)
        {
            if (instance != null)
            {
                Instance.message.Append(message + "\n");
            }
        }

        public static void PrintFloat(string message, float f)
        {
            if (instance != null)
            {
                Instance.message.Append(message + ": " + f + "\n");
            }
        }

        public static void PrintVector(string message, Vector3 v)
        {
            if (instance != null)
            {
                Instance.message.Append(message + ": (" + v.x + ", " + v.y + ", " + v.z + ")\n");
            }
        }

        // Update is called once per frame
        void Update()
        {
            PrintMessage("Press F1 to toggle cam following");
            PrintMessage("Press F2 to speed up");
            PrintMessage("Press F3 to slow down");
            PrintMessage("Press F4 to toggle messages");
            PrintMessage("Press F5 to toggle vector drawing");
            PrintMessage("Press F6 to toggle debug drawing");
            PrintMessage("Press F7 to level camera");
            PrintMessage("Press F8 to toggle cell space partitioning");
            PrintMessage("Press F9 to toggle non-penetration constraint");
            PrintMessage("Press F10 to toggle Rift");
            int fps = (int)(1.0f / Time.deltaTime);
            PrintFloat("FPS: ", fps);
            PrintMessage("Current scenario: " + currentScenario.Description());
            for (int i = 0; i < scenarios.Count; i++)
            {
                PrintMessage("Press " + i + " for " + scenarios[i].Description());
            }
            GameObject ovrplayer = GameObject.FindGameObjectWithTag("ovrcamera");
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            if (camFollowing)
            {
                camera.transform.position = camFighter.transform.position;
                camera.transform.rotation = camFighter.transform.rotation;

                if (ovrplayer != null)
                {
                    ovrplayer.transform.position = camFighter.transform.position;
                    ovrplayer.GetComponent<OVRCameraController>().SetOrientationOffset(camFighter.transform.rotation);
                }
            }
            else
            {
                if (ovrplayer != null)
                {
                    //ovrplayer.transform.position = camera.transform.position;
                    //ovrplayer.GetComponent<OVRCameraController>().transform.position = camera.transform.position;
                    //ovrplayer.GetComponent<OVRCameraController>().SetOrientationOffset(camera.transform.rotation);

                }
            }

            if (Params.cellSpacePartitioning)
            {
                PrintMessage("Cell space partitioning on");
            }
            else
            {
                PrintMessage("Cell space partitioning off");
            }

            if (Params.enforceNonPenetrationConstraint)
            {
                PrintMessage("Enforce non penetration constraint on");
            }
            else
            {
                PrintMessage("Enforce non penetration constraint off");
            }

            if (Params.drawDebugLines)
            {
                space.Draw();
            }
      
            currentScenario.Update();
        }

        void LateUpdate()
        {
            if (Params.cellSpacePartitioning)
            {
                space.Partition();                    
            }
        }
    }
}
