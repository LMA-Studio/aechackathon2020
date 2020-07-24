using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ShootingPaintScript : MonoBehaviour
{
    public XRNode inputSource;
    public XRRayInteractor rightInteractorRay;

    public float speed = 40;

    public GameObject paintBlob;
    public Transform barrel;
    public AudioSource audioSource;
    public AudioClip audioClip;

    private bool clicking = true;

    private void Update()
    {
        Vector3 pos = new Vector3();
        Vector3 norm = new Vector3();
        int index = 0;
        bool validTarget = false;

        bool isRightInteractorRayHovering = rightInteractorRay.TryGetHitInfo(ref pos, ref norm, ref index, ref validTarget);

        InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);

        bool clicked;
        device.TryGetFeatureValue(CommonUsages.triggerButton, out clicked);

        if (clicked)
        {
            if (!clicking)
            {
                clicking = true;
                if (!isRightInteractorRayHovering)
                {
                    Fire();
                }
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
        spawnedPaint.GetComponent<Rigidbody>().velocity = speed * barrel.forward;


        audioSource.PlayOneShot(audioClip);
        Destroy(spawnedPaint, 2);
    }

}
