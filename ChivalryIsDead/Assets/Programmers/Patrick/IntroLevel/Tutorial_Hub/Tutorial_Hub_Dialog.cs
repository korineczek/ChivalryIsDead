﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Tutorial_Hub_Dialog : MonoBehaviour {

    public GameObject UI;
    //GameMenu gameMenu;

    bool procceed;

    public GameObject HandCanvas;
    public GameObject ScreenFreeze;
    Animator handAnimator;
    public Animator swordAnimator;
    public Animator skipAnimator;
    int count;
    bool waitforClick;

    Animator BlackScreenAnimator;
    public GameObject blackScreen;
    float duration;
    public HubDataManager hdManager;

    // Use this for initialization
    void Start()
    {
        //gameMenu = UI.GetComponent<GameMenu>();
        BlackScreenAnimator = blackScreen.GetComponent<Animator>();
        count = 0;
        procceed = false;
        waitforClick = true;
        handAnimator = HandCanvas.GetComponent<Animator>();
        StartCoroutine("DialogOne");
    }

    // Update is called once per frame
    void Update()
    {
        if (!waitforClick)
        {
            if (hdManager.isClicked)
            {
                StartCoroutine("DialogThree");
                waitforClick = true;
                hdManager.isClicked = false;
            }
        }
    }

    public IEnumerator DialogOne()
    {   
        //yield return new WaitForSeconds(1f);
        this.gameObject.GetComponent<DialogObject>().StartCoroutine("DialogSystem", 0);

        while(count < 2)
        {
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitUntil(SkipAndPlay);

        procceed = false;
        BlackScreenAnimator.SetTrigger("fadeOut");
        duration = BlackScreenAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duration);
        StartCoroutine("DialogTwo");

    }

    public bool SkipAndPlay()
    {
        return procceed;
    }

    public void CallableSkip()
    {
        procceed = true;
        count++;
    }

    public IEnumerator DialogTwo()
    {
        //yield return new WaitForSeconds(1f);
        this.gameObject.GetComponent<DialogObject>().StartCoroutine("DialogSystem", 1);

        count = 0;
        while (count < 2)
        {
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitUntil(SkipAndPlay);

        procceed = false;
        blackScreen.SetActive(false);
        waitforClick = false;
    }

    public IEnumerator DialogThree()
    {
        yield return new WaitForSeconds(1f);
        this.gameObject.GetComponent<DialogObject>().StartCoroutine("DialogSystem", 2);

        count = 0;
        while (count < 2)
        {
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitUntil(SkipAndPlay);

        procceed = false;
        handAnimator.SetBool("handHub", true);

    }

    public void LoadTutorial2()
    {
        handAnimator.SetBool("handHub", false);
        SceneManager.LoadScene(4);
    }


}
