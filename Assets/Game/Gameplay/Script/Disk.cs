using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Funzilla;

public class Disk: MonoBehaviour
{
    private float time;
    private Vector3 rotation;
    private float RotateAmount = 2f;
    private float vitri;
    private float gocquay = 90f;
    private float rotationMove;
    [SerializeField] private GameObject diskWin;
    [SerializeField] private Gameplay gamePlay;
    void Start(){
       DiskWin();
    }
    void Update()
    {
        rotationMove = gocquay * Time.deltaTime;
        transform.Rotate(0,  rotationMove, 0); 
    }
    void DiskWin()
    {
        int WinDiskPosition = gamePlay.DiskList.Count - 2;
        diskWin.transform.position = new Vector3(0,- WinDiskPosition * 1.5f - 0.6f,0);
        diskWin.transform.localScale = new Vector3(16f, 1.2f, 16f);
    }
}