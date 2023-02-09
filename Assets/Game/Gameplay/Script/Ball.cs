using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.SocialPlatforms;
using  Funzilla;
using GameAnalyticsSDK.Setup;

public class Ball : MonoBehaviour
{
    private const float g = 9.8f;
    private const float _v0 = 15.5f;
    private float _v;
    private float _vSmash = 27f;
    private bool checkClicking;
    // private float _smax = g * _v0;
    private float _smax = 3f;
    private float _t = 0;
    private float yFall = 0;
    private float yJump =0;
    private float _s0 = 0;
    private int dem = 0;
    private float dr;
    private float vantocroi;
    private float v;
    private Vector3 tempVec;
    private bool furing = false;
    private bool checkFurry = false;
    private bool checkSmash;
    public State _currentState = State.Jump;
    private int _undestroyable = 1;
    private float speedDestroy;
    private GameObject destroyDisk;
    private Vector3 _originScale;
    private Vector3 _scaleTo;
    [SerializeField] private GameObject pieceBall;
    [SerializeField] private Gameplay disks;
    [SerializeField] private Image whiteCircle;
    [SerializeField] private GameObject FireFury;
    [SerializeField] private GameObject ObjectParent;
    public enum State
    {
        Jump,Fall,Smash,Die
    }

    void Start()
    {
        checkClicking = false;
        FireFury.SetActive(false);
        pieceBall.SetActive(false);
        gameObject.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            // tempVec = new Vector3(0,-1f * _vSmash,0 ); 
            // transform.GetComponent<Rigidbody>().MovePosition(transform.position + tempVec);
            transform.Translate(transform.up * -27f *Time.deltaTime);
        }
        switch (_currentState)
        {
            case State.Jump:
                checkSmash = false;
                //TODO: QUA BONG NHAY LEN
                yJump = _s0 + _v * _t + 0.5f * g * _t * _t;
                transform.position = new Vector3(0, yJump, 4.5f);
                
                //TODO: QUA BONG BIEN DANG KHI NHAY LEN
                transform.localScale = new Vector3(1.35f - 0.9f * _t, 1.35f + 0.3f * _t,1.35f - 0.9f * _t);
                
                //TODO: PHAT HIEN THOI DIEM ROI XUONG
                if(yJump > _smax && _currentState != State.Smash)
                {
                    ChangeSate(State.Fall);
                    return;
                }
                if (Input.GetMouseButtonDown(0) && disks.DiskList.Count > 2f && checkClicking != true)
                {
                    ChangeSate(State.Smash);
                    return;
                }
                break;
            case State.Fall:
                //TODO: QUA BONG ROI XUONG
                checkSmash = false;
                yFall = _s0 + _v * _t - 0.5f * 2f * g * _t * _t;
                transform.position = new Vector3(0, yFall, 4.5f);
                
                //TODO: QUA BONG BIEN DANG KHI ROI
                transform.localScale = new Vector3(1.35f - 0.35f * _t, 1.35f + 0.2f * _t,1.35f - 0.35f * _t);
                
                if (Input.GetMouseButtonDown(0) && disks.DiskList.Count > 2f && checkClicking != true)
                {
                    ChangeSate(State.Smash);
                    return;
                }
                break;
            case State.Smash:
                if (Input.GetMouseButton(0) && checkClicking != true)
                {
                    float countdownFurry = 4.5f * Time.deltaTime;
                    int count = 0;
                    if (transform.position.y < disks.DiskList[0].transform.position.y && disks.DiskList.Count > 2)
                    {
                        theDestroy();
                        Destroy(disks.DiskList[0]);
                        disks.DiskList.Remove(disks.DiskList[0]);
                        count++;
                        if (furing == false)
                        {
                            whiteCircle.fillAmount += countdownFurry;
                        }
                        _undestroyable = 1;
                    }
                    _smax -= count * 1.5f;
                }
                if (whiteCircle.fillAmount == 1f)
                {
                    furing = true;
                    FireFury.SetActive(true);
                    checkFurry = true;
                }

                if (whiteCircle.fillAmount <= 0)
                {
                    furing = false;
                    FireFury.SetActive(false);
                    checkFurry = false;
                }
                break;
            case State.Die:
                break;
            default:
                return;
        }
        if (whiteCircle.fillAmount <= 0)
        {
            furing = false;
            FireFury.SetActive(false);
            checkFurry = false;
        }
        if (Input.GetMouseButtonUp(0))
        {
            ChangeSate(State.Jump);
            return;
        }
        pieceBall.transform.position = transform.position;
        _t += Time.deltaTime;
        whiteCircle.fillAmount -= 0.5f * Time.deltaTime;
        
    }
    private void ChangeSate(State state)
    {
        if (state == _currentState) return;
        _currentState = state;
        switch (state)
        {
            case State.Jump:
                _s0 = transform.position.y;
                _v = _v0;
                _t = 0;
                break;
            case State.Fall:
                _s0 = transform.position.y;
                _v = 6f;
                _t = 0;
                break;
            case State.Smash:
                break;
            case State.Die:
                checkClicking = true;
                StartCoroutine(delayCreat());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void theDestroy()
    {
            List<Transform> parent = new List<Transform>();
            for (int i = 0; i < disks.DiskList[0].transform.childCount; i++)
            {
                parent.Add(disks.DiskList[0].transform.GetChild(i));
            }
            List<Transform> childList = new List<Transform>();
            foreach (Transform child in parent)
            {
                Transform childDisk = Instantiate(child,disks.DiskList[0].transform.position + Vector3.up*1.5f, child.transform.rotation);
                childDisk.localScale = new Vector3(2f, 1f, 2f);
                childDisk.transform.parent = ObjectParent.transform ;
                childList.Add(childDisk);
                Rigidbody rb = childDisk.gameObject.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.AddForce(0,6f,-8f,ForceMode.Impulse);
            }
            
            for (int i = 0; i < 4; i++)
            {
                int temp = UnityEngine.Random.Range(0, childList.Count - 1);
                childList[temp].gameObject.GetComponent<Rigidbody>().AddForce(7f - temp,5f,0,ForceMode.Impulse);
                childList[temp+1].gameObject.GetComponent<Rigidbody>().AddForce(-7f - temp,1f+temp,0,ForceMode.Impulse);
            }
    }

    private void OnTriggerEnter(Collider other)
    {
            if (_currentState == State.Fall)
            {
                transform.localScale = new Vector3(1f +  0.3f*_t,1f - 0.05f * _t,1f);
                ChangeSate(State.Jump);
                return;
            }

            if (_currentState == State.Smash)
            {
                checkSmash = true;
            }
            if(_currentState == State.Smash && other.gameObject.CompareTag("Black_Piece") && checkFurry == false)
            {
                if (_undestroyable == 1)
                {
                    ChangeSate(State.Jump);
                    _originScale = disks.DiskList[0].transform.localScale;
                    _scaleTo = _originScale * 1.5f;
                    disks.DiskList[0].transform.DOScale(_scaleTo, 0.3f)
                        .SetEase(Ease.InOutSine)
                        .OnComplete(() =>
                        {
                            disks.DiskList[0].transform.DOScale(_originScale, 0.3f)
                                .SetEase(Ease.OutBounce);
                        } );
                    _undestroyable--;
                }
                else
                {
                    ChangeSate(State.Die);
                }
            }
            if (other.gameObject.CompareTag("Win_Piece") && disks.DiskList.Count == 2)
            {
                checkClicking = true;
                ChangeSate(State.Jump);
                disks.ChangeState(Gameplay.GameStates.Win);
            }
    }
    IEnumerator delayCreat(){
        yield return new WaitForSeconds(0.4f);
        disks.ChangeState(Gameplay.GameStates.Lose);
    }
} 