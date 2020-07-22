using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintWandEndController : MonoBehaviour
{
    private bool selectingMaterials = false;
    
    public void StartSelecting ()
    {
        selectingMaterials = true;
    }

    public void EndSelecting()
    {
        selectingMaterials = false;
    }

    public void SelectMaterial (Material selectedMaterial)
    {
        this.GetComponent<MeshRenderer>().material = selectedMaterial;

        GameObject parent = this.transform.parent.gameObject;
        ShootingPaintScript paintShooter = parent.GetComponent<ShootingPaintScript>();
        paintShooter.ChangePaint(selectedMaterial);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Colliding " + collision.gameObject.name);

        if (collision.gameObject.tag == "paint")
        {
            GameObject parent = this.transform.parent.gameObject;
            ShootingPaintScript paintShooter = parent.GetComponent<ShootingPaintScript>();

            if(selectingMaterials)
            {
                Material newMaterial = this.GetComponent<MeshRenderer>().material;
                collision.gameObject.GetComponent<MeshRenderer>().material = newMaterial;

                paintShooter.ChangePaint(newMaterial);
            }

            else
            {
                Debug.Log("Attaching Paint");

                Material paintMaterial = collision.gameObject.GetComponent<MeshRenderer>().material;

                paintShooter.ChangePaint(paintMaterial);
                this.GetComponent<MeshRenderer>().material = paintMaterial;
            }
        }
    }
}
