﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class GameBehavior : MonoBehaviour
{
    public Transform wall;
    public List<Transform> wallCells;
    public GameObject doorFrame;
    public GameObject chest;
    public GameObject keycard;
    public GameObject dialogBox;

    public Camera cam;

    private bool _isGameOnPause;
    private bool _doorOpened;
    private bool _chestOpened;
    private bool _keyPicked;
    private GameObject _currentActiveObject;

    enum DialogType { CHESTOPENED = 1, KEYPICKED = 2, DOOROPENED = 3 };
    private DialogType _currentDialog = 0;


    private void Start()
    {
        PauseGame();
    }
    // Start is called before the first frame update
    public void StartGame()
    {
        UnpauseGame();
        _doorOpened = false;
        _chestOpened = false;
        _keyPicked = false;
        InitialiseWallCells();
        RandomlyPlaceDoor();
        RandomlyPlaceChestAndKey();
    }

    public void InitialiseWallCells()
    {
        wallCells.Clear();
        foreach(Transform clild in wall)
        {
            wallCells.Add(clild);
        }
    }
    public void RandomlyPlaceDoor()
    {
        int randomIndex = Random.Range(0, wallCells.Count);
        Transform wallCellInfo = wallCells[randomIndex].transform;
        Destroy(wallCells[randomIndex].gameObject);
        Instantiate(doorFrame, wallCellInfo.position, wallCellInfo.rotation);
    }

    public void RandomlyPlaceChestAndKey()
    {
        bool hasCollision = true;
        for (int attempt = 0; attempt < 10 && hasCollision; attempt++)
        {
            float x = Random.Range(-6f, 6f);
            float z = Random.Range(-6f, 6f);
            Vector3 spawnPosition = new Vector3(x, 0, z);
            Collider[] colliders = Physics.OverlapBox(spawnPosition, new Vector3(0.5f, 0f, 0.5f), Quaternion.identity, LayerMask.GetMask("Props"));
            Debug.Log(colliders.Length);
            hasCollision = false;
            foreach (Collider col in colliders)
            {
                if (col)
                {
                    hasCollision = true;
                    break;
                }
            }

            if (!hasCollision)
            {
                if (spawnPosition.z > 0)
                    Instantiate(chest, spawnPosition, Quaternion.Euler(0, 180, 0));
                else
                    Instantiate(chest, spawnPosition, Quaternion.identity);

                Instantiate(keycard, spawnPosition + new Vector3(0f, 0.15f, 0f), Quaternion.Euler(-90,90,0));
            }
        }
    }

    void FixedUpdate()
    {
        if(!_isGameOnPause)
            CastRay();
    }

    public void CastRay(bool isMouseClickAction = false)
    {
        RaycastHit objectHit;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Physics.Raycast(ray, out objectHit);

        if(objectHit.distance < 3)
        {
            if (!_doorOpened && objectHit.transform.gameObject.name == "Door")
            {
                if (isMouseClickAction)
                {
                    PauseGame();
                    if (_keyPicked)
                    {
                        _currentDialog = DialogType.DOOROPENED;
                        _currentActiveObject = objectHit.transform.gameObject;
                        dialogBox.GetComponent<DialogManager>().ShowDialogBox("Open?", 2, "Yes", "No");
                    }
                    else
                    {
                        dialogBox.GetComponent<DialogManager>().ShowDialogBox("You need a key!", 1, "Ok");
                    }
                }
            }

            Debug.Log(_chestOpened + objectHit.transform.gameObject.name);
            if (!_chestOpened && objectHit.transform.gameObject.name == "Chest")
            {
                Debug.Log("Got a chest!");
                if (isMouseClickAction)
                {
                    _currentActiveObject = objectHit.transform.gameObject;
                    _currentDialog = DialogType.CHESTOPENED;
                    PauseGame();
                    dialogBox.GetComponent<DialogManager>().ShowDialogBox("Open?", 2, "Yes", "No");
                }
            }

            if (!_keyPicked && objectHit.transform.gameObject.name == "Keycard(Clone)")
            {
                if (isMouseClickAction)
                {
                    PauseGame();
                    _currentDialog = DialogType.KEYPICKED;
                    _currentActiveObject = objectHit.transform.gameObject;
                    dialogBox.GetComponent<DialogManager>().ShowDialogBox("Take?", 2, "Yes", "No");
                }
            }
        }
    }


    public void PauseGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _isGameOnPause = true;
    }

    public void UnpauseGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _isGameOnPause = false;
    }

    public bool IsGameOnPause()
    {
        return _isGameOnPause;
    }

    public void HandleDialog()
    {
        switch (_currentDialog)
        {
            case DialogType.CHESTOPENED:
                _currentActiveObject.GetComponent<ChestBehavior>().OpenChest();
                _chestOpened = true;
                UnpauseGame();
                break;
            case DialogType.KEYPICKED:
                _keyPicked = true;
                Destroy(_currentActiveObject);
                UnpauseGame();
                break;
            case DialogType.DOOROPENED:

                break;
        }

    }
}
