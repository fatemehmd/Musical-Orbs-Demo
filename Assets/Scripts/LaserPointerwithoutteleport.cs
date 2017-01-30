﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointerwithoutteleport : MonoBehaviour {

    public Transform cameraRigTransform;
    public Transform FlyingTransform;
    public Transform headTransform; // The camera rig's head
    public Vector3 teleportReticleOffset; // Offset from the floor for the reticle to avoid z-fighting
    public LayerMask teleportMask; // Mask to filter out areas where teleports are allowed

    private SteamVR_TrackedObject trackedObj;

    public GameObject laserPrefab; // The laser prefab
    private GameObject laser; // A reference to the spawned laser
    private Transform laserTransform; // The transform component of the laser for ease of use

    public GameObject teleportReticlePrefab; // Stores a reference to the teleport reticle prefab.
    private GameObject reticle; // A reference to an instance of the reticle
    private Transform teleportReticleTransform; // Stores a reference to the teleport reticle transform for ease of use

    public GameObject flyingPrefab;
    private GameObject flying;

    private Vector3 hitPoint; // Point where the raycast hits
    public bool shouldTeleport; // True if there's a valid teleport target

    

    //public Transform endMarker;
    public float speed = 1.0F;
    private float startTime;
    private float journeyLength;
   
    private bool _needMove = false;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    //new
    void Start()
    {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
        reticle = Instantiate(teleportReticlePrefab);
        teleportReticleTransform = reticle.transform;
        flying = Instantiate(flyingPrefab);
        FlyingTransform = flying.transform;
        
    }

    void Update()
    {
        // Is the touchpad held down?
        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
        {
            RaycastHit hit;

            // Send out a raycast from the controller
            if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 1000, teleportMask))
            {
                hitPoint = hit.point;
              //  endMarker.position = hit.point;
                ShowLaser(hit);

                //Show teleport reticle
                reticle.SetActive(true);
                teleportReticleTransform.position = hitPoint + teleportReticleOffset;

                shouldTeleport = true;
            }
        }
        else // Touchpad not held down, hide laser & teleport reticle
        {
            laser.SetActive(false);
            reticle.SetActive(false);
        }

        // Touchpad released this frame & valid teleport position found
        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad) && shouldTeleport)
        {
            startTime = Time.time;
            journeyLength = Vector3.Distance(this.transform.position, hitPoint);
            _needMove = true;
        }

        if (!_needMove) return;

        float distCovered = (Time.time - startTime) * speed;
        float fracJourney = distCovered / journeyLength;
        FlyingTransform.position = Vector3.Lerp(this.transform.position, hitPoint, fracJourney);
        Debug.Log(FlyingTransform.position);
        //Debug.Log(cameraRigTransform.position);

    }
    

    private void ShowLaser(RaycastHit hit)
    {
        laser.SetActive(true); //Show the laser
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f); // Move laser to the middle between the controller and the position the raycast hit
        laserTransform.LookAt(hitPoint); // Rotate laser facing the hit point
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y,
            hit.distance); // Scale laser so it fits exactly between the controller & the hit point
    }

    private void Teleport()
    {
        shouldTeleport = false; // Teleport in progress, no need to do it again until the next touchpad release
        reticle.SetActive(false); // Hide reticle
                                  // Vector3 difference = cameraRigTransform.position - headTransform.position; // Calculate the difference between the center of the virtual room & the player's head
                                  // difference.y = 0; // Don't change the final position's y position, it should always be equal to that of the hit point
                                  // cameraRigTransform.position = hitPoint + difference; // Change the camera rig position to where the the teleport reticle was. Also add the difference so the new virtual room position is relative to the player position, allowing the player's new position to be exactly where they pointed. (see illustration)

    }
}

