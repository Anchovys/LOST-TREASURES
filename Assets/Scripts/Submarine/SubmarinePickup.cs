﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class SubmarinePickup : NetworkBehaviour
{
    [SerializeField]
    private float timeForPickup = 2;

    public Treasure Treasure;
    
    [SerializeField]
    private Transform _treasureSpawnPoint;
    
    private Transform _transform;
    private OverlayUI _UI;

    private Submarine _submarine;

    [SerializeField] private float _pickupTimer;
    private float PickUpTimer 
    { 
        get  { return _pickupTimer; }
        set
        {
            _pickupTimer = value;
            UpdateTimerProgress();
        }
    }

    public UnityAction<Treasure> pickedUpTreasure;
    public UnityAction<Treasure> droppedTreasure;
    
    private bool _pressed;

    #region Timeout Optimize
    // 10 calls per second
    private float updateInterval = 0.1f;
    private float _updateDeltaTime;

    #endregion

    private void Start()
    {
        _UI = GameObject.FindGameObjectWithTag("UI").GetComponent<OverlayUI>();
        _submarine = GetComponent<Submarine>();
    }

    private void UpdateTimerProgress()
    {
        _UI.SetActionPercentage(_pickupTimer / timeForPickup);
    }

    private void Awake()
    {
        _transform = transform;
    }

    private void Update()
    {
        if (_submarine.Lock)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Treasure != null)
            {
                DropTreasure();
                return;
            }
            _pressed = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            _pressed = false;
            PickUpTimer = 0;
            return;
        }

        OptimizedUpdate();
        
    }

    private void OptimizedUpdate()
    {
        if (_updateDeltaTime >= updateInterval)
        {
            _updateDeltaTime = 0;
            if (_pressed)
                WaitForPickUp();
        }
        else
        {
            _updateDeltaTime += Time.deltaTime;
        }
    }

    private void WaitForPickUp()
    {
        if (Physics.Raycast(new Ray(_transform.position, _transform.up * -1), out RaycastHit hitInfo, 2))
        {
            var treasure = hitInfo.transform.GetComponent<Treasure>();

            if (treasure != null)
            {
                if (PickUpTimer < timeForPickup)
                {
                    PickUpTimer += updateInterval;
                    return;
                }

                Treasure = treasure;
                pickedUpTreasure(treasure);
                CmdPickUp(_submarine.netId, treasure.netId);

                PickUpTimer = 0;
                return;
            }
        }

        if (PickUpTimer > 0)
        {
            PickUpTimer -= updateInterval * 2;
        }
    }

    public void DropTreasure()
    {
        if (Treasure == null)
            return;
        droppedTreasure(Treasure);
        CmdDrop(Treasure.netId);
        Treasure = null;
    }

    [Command]
    public void CmdPickUp(NetworkInstanceId playerId, NetworkInstanceId treasureId)
    {
        if (isServer)
        {
            Debug.LogError("Pick up player " + playerId);
            ClientScene.FindLocalObject(treasureId).GetComponent<Treasure>().RpcPickup(playerId);
        }

    }

    [Command]
    public void CmdDrop(NetworkInstanceId treasureId)
    {
        if (isServer)
        {
            ClientScene.FindLocalObject(treasureId).GetComponent<Treasure>().RpcDrop();
        }
    }
}
