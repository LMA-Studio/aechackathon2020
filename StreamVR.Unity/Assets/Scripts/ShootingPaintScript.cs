using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ShootingPaintScript : MonoBehaviour
{
    public XRNode inputSource;

    public float speed = 40;

    public GameObject paintBlob;
    public Transform barrel;
    public AudioSource audioSource;
    public AudioClip audioClip;

    private bool clicking = true;

    private void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);

        bool clicked;
        device.TryGetFeatureValue(CommonUsages.triggerButton, out clicked);

        if (clicked)
        {
            if (!clicking)
            {
                clicking = true;
                Fire();
            }
        }
        else
        {
            clicking = false;
        }
    }

    public void ChangePaint(Material newPaint)
    {
        paintBlob.GetComponent<MeshRenderer>().material = newPaint;
    }

    public void Fire()
    {
        GameObject spawnedPaint = Instantiate(paintBlob, barrel.position, barrel.rotation);
        spawnedPaint.GetComponent<Rigidbody>().velocity = speed * barrel.up;
        audioSource.PlayOneShot(audioClip);
        Destroy(spawnedPaint, 2);
    }

}
