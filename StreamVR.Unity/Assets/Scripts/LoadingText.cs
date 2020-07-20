using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingText : MonoBehaviour
{
    System.Random rand;
    List<string> actions = new List<string>();
    public TMPro.TMP_Text loadingText;
    // Start is called before the first frame update
    void Start()
    {
        
        int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
        rand = new System.Random(seed);
        StartCoroutine(LoopDisplayMessage());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator LoopDisplayMessage() 
    {
        for(;;) 
        {
            int randNumber = rand.Next(1, 17);
            loadingText.text = actions[randNumber];
            yield return new WaitForSeconds(5f);
        }
    }

    public List<string> ListOfActions()
    {
        actions.Add("ASSEMBLING FURNITURE");
        actions.Add("PAINTING WALLS");
        actions.Add("INSTALLING FLOORING");
        actions.Add("INSTALLING LIGHT BULBS");
        actions.Add("POURING SLAB");
        actions.Add("FRAMING WALLS");
        actions.Add("RUNNNING CONDUIT");
        actions.Add("HANGING THE CURTAINS");
        actions.Add("CLEANING THE WINDOWS");
        actions.Add("SWEEPING UP THE DUST");
        actions.Add("POSITIONING THE DOORS");
        actions.Add("LOOKING FOR THE HAMMER");
        actions.Add("TESTING THE SPEAKERS");
        actions.Add("SELECTING A SITE");
        actions.Add("TIGHTENING SOME SCREWS");
        actions.Add("LOST THE KEYS");
        actions.Add("FOUND THE KEYS");

        return actions;
    }


}
