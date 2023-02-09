using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using Random = UnityEngine.Random;
using Funzilla;

namespace Funzilla
{
    internal class Gameplay : Scene
    {
        public List<GameObject> DiskList = new List<GameObject>();
        private GameStates _currentGameState = GameStates.Init;
        public GameObject startingText;
        public GameObject NextLevelText;
        public GameObject LoseText;
        [SerializeField] private Text levelText;
        [SerializeField] private GameObject[] NextDisk;
        [SerializeField] private GameObject ObjectParent;
        private GameObject ArrayDisk;
        private GameObject obstacle1;
        public static int GainLevel = 50;
        private Color randomColor;
        private float lechgoc;
        private float _d = 1.5f;
        private float _r = 90f;
        private float _v = 27f;
        private static int checkWin = 0;
        internal Level Level { get; private set; }
        public static Gameplay Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public enum GameStates
        {
            Init,
            Next,
            Win,
            Lose
        }

        private void Start()
        {
            GameManager.Init(Init);
            levelText.text = $"Level {Profile.Level}";
            lechgoc = _d * _r / _v;  
            for (int i = 0; i < DiskList.Count; i++)
            {
                DiskList[i].transform.localEulerAngles = new Vector3(0, -lechgoc * i, 0);
            }
            Debug.Log("bug"+lechgoc);
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
               startingText.SetActive(false);
            }

            switch (_currentGameState)
            {
                case GameStates.Lose:
                    LoseText.SetActive(true);
                    if (Input.GetMouseButton(0))
                    {
                        for (int i = 0; i < DiskList.Count; i++)
                        {
                            Destroy(DiskList[i]);
                        }
                        DiskList.Clear();
                        if (DiskList.Count == 0)
                        {
                            SceneManager.ReloadScene(SceneID.Gameplay);
                        }
                        LoseText.SetActive(false);
                    }
                    break;
                case GameStates.Win:
                    if (Input.GetMouseButtonDown(0) && DiskList.Count == 2)
                    {
                        NextLevelText.SetActive(false);
                        ChangeState(GameStates.Next);
                    } 
                    break;
                default:
                    return;
            }
        }

        //Start make Disk
        public void CreatDisk()
        {
            int RandomDisk = Random.Range(0, 3);
            int countPiece = NextDisk[RandomDisk].transform.childCount;
            //Start born Disk
            for (int i = 0; i < GainLevel + Profile.GainLevel; i++)
            {
                GameObject cloneDisk = Instantiate(NextDisk[RandomDisk], new Vector3(0, i * -1.5f, 0), Quaternion
                    .Euler(new Vector3(0, 0, 0)));
                DiskList.Add(cloneDisk);
            }
            //End born Disk
            //Start hard level
            if (Profile.Level <= 5)
            {
                for (int i = 0; i < DiskList.Count; i += 4)
                {
                    obstacle1 = DiskList[i].transform.GetChild(0).GetChild(0).gameObject;
                    obstacle1.tag = "Black_Piece";
                    Color customColor = new Color(0.12f, 0.12f, 0.12f, 1f);
                    obstacle1.GetComponentInChildren<MeshRenderer>().material.color = customColor;
                }
                for (int i = 0; i < DiskList.Count; i++)
                {
                    for (int j = 0; j < countPiece; j++)
                    {
                        obstacle1 = DiskList[i].transform.GetChild(j).GetChild(0).gameObject;
                        if (!obstacle1.CompareTag("Black_Piece"))
                        {
                            obstacle1.GetComponentInChildren<MeshRenderer>().material.color = randomColor;
                        }
                    }
                }
            }
            
            if (Profile.Level > 5)
            {
                for (int i = 0; i < (int) (DiskList.Count / 7); i++)
                { 
                    obstacle1 = DiskList[i].transform.GetChild(0).GetChild(0).gameObject;
                    obstacle1.tag = "Black_Piece";
                    obstacle1.GetComponentInChildren<MeshRenderer>().material.color = Color.grey;
                }
            
                for (int i = (int) (DiskList.Count / 7); i < (int) (DiskList.Count * 2 / 7); i += 2)
                {
                    for (int j = 0; j < countPiece - countPiece / 2; j++)
                    {
                        obstacle1 = DiskList[i].transform.GetChild(j).GetChild(0).gameObject;
                        obstacle1.tag = "Black_Piece";
                        obstacle1.GetComponentInChildren<MeshRenderer>().material.color = Color.grey;
                    }
                }
            
                for (int i = (int) (DiskList.Count * 2 / 7); i < DiskList.Count * 3 / 7; i++)
                {
                    for (int j = 2; j < countPiece - 1; j++)
                    {
                        obstacle1 = DiskList[i].transform.GetChild(j).GetChild(0).gameObject;
                        obstacle1.tag = "Black_Piece";
                        obstacle1.GetComponentInChildren<MeshRenderer>().material.color = Color.gray;
                    }
                }
            
                for (int i = (int) (DiskList.Count * 3 / 7); i < DiskList.Count - 2; i++)
                {
                    int RandomPiece2 = Random.Range(0, countPiece - 1);
                    obstacle1 = DiskList[i].transform.GetChild(RandomPiece2).GetChild(0).gameObject;
                    obstacle1.tag = "Black_Piece";
                    obstacle1.GetComponentInChildren<MeshRenderer>().material.color = Color.gray;
                }
            
                for (int i = 0; i < DiskList.Count; i++)
                {
                    for (int j = 0; j < countPiece; j++)
                    {
                        obstacle1 = DiskList[i].transform.GetChild(j).GetChild(0).gameObject;
                        if (!obstacle1.CompareTag("Black_Piece"))
                        {
                            obstacle1.GetComponentInChildren<MeshRenderer>().material.color = randomColor;
                        }
                    }
                }
            }
        }
        private void Init()
        {
            randomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            CreatDisk();
            for (int i = 0; i < 4; i++)
            {
                NextDisk[i].SetActive(false);
            }
            startingText.SetActive(true);
            NextLevelText.SetActive(false);
            LoseText.SetActive(false);
            SceneManager.HideLoading();
            SceneManager.HideSplash();
        }

        public void ChangeState(GameStates newState)
        {
            if (newState == _currentGameState) return;
            ExitCurrentState();
            _currentGameState = newState;
            EnterNewState();
        }

        private void EnterNewState()
        {
            switch (_currentGameState)
            {
                case GameStates.Init:
                    break;
                case GameStates.Next:
                    Profile.Level += 1;
                    if (Profile.GainLevel >= 300) Profile.GainLevel = 300;
                    else Profile.GainLevel += 5;
                    if (Input.GetMouseButton(0))
                    {
                        for (int i = 0; i < DiskList.Count; i++)
                        {
                            Destroy(DiskList[i]);
                        }

                        DiskList.Clear();
                        if (DiskList.Count == 0)
                        {
                            SceneManager.ReloadScene(SceneID.Gameplay);
                        }
                        checkWin += 1;
                    }
                    break;
                case GameStates.Win:
                    Destroy(ObjectParent);                   
                    NextLevelText.SetActive(true);
                    break;
                case GameStates.Lose:
                    Destroy(ObjectParent);
                    LoseText.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ExitCurrentState()
        {
            switch (_currentGameState)
            {
                case GameStates.Init:
                    break;
                case GameStates.Next:
                    checkWin+=1;
                    break;
                case GameStates.Win:
                    break;
                case GameStates.Lose:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
