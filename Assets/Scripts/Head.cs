using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


[Serializable]
public class Eyes
{
    [SerializeField] private Transform rightEye, leftEye;
    public void EyeLookAt(Vector3 target)
    {
        target.y = rightEye.position.y;
        rightEye.LookAt(target);
        var angel = rightEye.eulerAngles;
        angel += new Vector3(90, 0, 0);
        angel.y = Mathf.Clamp(angel.y, 150, 200);
        leftEye.eulerAngles = rightEye.eulerAngles = angel;
    }
    public void EyeRotation()
    {
        rightEye.rotation = Quaternion.Lerp( rightEye.rotation,Quaternion.Euler(-90,0,0), 6* Time.deltaTime);
        leftEye.rotation = Quaternion.Lerp( leftEye.rotation,Quaternion.Euler(-90,0,0), 6* Time.deltaTime);
    }
}

[Serializable]
public class JumpPoint
{
    [SerializeField ]private Transform jupPlatforms;
    private int index;
    public Vector3 GetJupPoint(out bool endJump)
    {
        index++;
        if (jupPlatforms.childCount >= index)
        {
            endJump = false;
            return jupPlatforms.GetChild(index -1).position;
        }
        
        endJump = true;
        return Vector3.zero;
    }

    public bool IsNull()
    {
        return jupPlatforms == null;
    }
}

public class Head : MonoBehaviour
{
    private float badStuffRate;
    public JumpPoint path;
    [SerializeField] private Transform topBone;
    [SerializeField] private SkinnedMeshRenderer mesh;
    [SerializeField] private new Collider collider;
    [SerializeField] private DOTweenAnimation doTween, sayNo;
    [SerializeField] private Eyes eyes;
    [HideInInspector] public int height;
    [HideInInspector] public bool full;
    private float  defaultHeight = 0.6f, waveStringht, sandwichPartHeight = 0.2f, eatRate, shakeHeadRate;
    private bool eating, eatingAll, lose, win, playerGetPath;

    private void Start()
    {
        sandwichPartHeight = Sandwich.Instance.height;
        GameManager.Instance.heads++;
        height = (int)((topBone.localPosition.y - defaultHeight) / sandwichPartHeight);
    }


    public void SetBadStuffRate(float rate)
    {
        if (eating || eatingAll || lose) return;
        badStuffRate = rate;
    }
    private void Update()
    { 
        HeadEmotion();
        MouthMovement(); 
        Eyes();
        ShakeHead();
        Dizzy();
        SetPathToPlayer();
    }

    
    private void HeadEmotion()
    {
        if (!lose)
        {
            mesh.SetBlendShapeWeight(0,badStuffRate * 100f);
        }
        else
        {
            mesh.SetBlendShapeWeight(0,Mathf.Lerp(mesh.GetBlendShapeWeight(0),0,6*Time.deltaTime));
            mesh.SetBlendShapeWeight(1,Mathf.Lerp(mesh.GetBlendShapeWeight(1),100,6*Time.deltaTime));
        }
    }
    private void ShakeHead()
    {
        if (!IsBad() || lose || eating || eatingAll)
        {
            if(eating || eatingAll) 
                sayNo.transform.rotation = Quaternion.Lerp(sayNo.transform.rotation,Quaternion.identity, 6*Time.deltaTime);
            return;
        }
        if (shakeHeadRate < 0)
        {
            shakeHeadRate = 1.5f;
            sayNo.DORestart();
        }
        else
        {
            shakeHeadRate -= Time.deltaTime;
        } 
    }
    private void Eyes()
    {
        if (!eating && !lose)
        {
            eyes.EyeLookAt(Sandwich.Instance.transform.position);
        }
        else
        {
            eyes.EyeRotation();
        }
    }
    private void MouthMovement()
    {
        var position = topBone.localPosition;
        var h = defaultHeight + height * sandwichPartHeight;
        waveStringht = Mathf.Lerp(waveStringht, 0, 3* Time.deltaTime);
        if(waveStringht <= 0.1f)eating = false;
        h += Mathf.Sin(Mathf.PI * Time.time*8) /4 * waveStringht;
        if (h < defaultHeight) h = defaultHeight;
        var target = new Vector3(position.x,h, position.z);
        topBone.localPosition = Vector3.Lerp(position, target, 6 * Time.deltaTime);
    }


    #region Eat
    public bool Eat()
    {
        if (height <= 1) Done();
        eating = true;
        sayNo.DOPause();
        waveStringht = 1f;
        height--;
        return height <= 0;
    }
    public void EatAll(List<Transform> sandwichParts)
    {
        full = true;
        eating = eatingAll = true;
        sayNo.DOPause();
        Grade grade = GameManager.Instance.GetActivePart().grade;
        grade.RemovePoint(grade.GetClosestPoint(transform.position));
        StartCoroutine(Eating(sandwichParts));
    }
    private IEnumerator Eating(List<Transform> sandwichParts)
    {
        while (true)
        {
            yield return new WaitForSeconds(0);
            if (height <= 0)
            {
                Done();
                for (int i = 0; i < sandwichParts.Count; i++)
                {
                    Destroy(sandwichParts[i].gameObject);
                }
                sandwichParts.Clear();
                break;
            }
            for (int i = 0; i < sandwichParts.Count; i++)
            {
                var target = i == 0 ? transform.position : sandwichParts[i-1].position;
                var waveDir = new Vector3();
                var h = sandwichPartHeight;
                if (i == 0) h = 0;
                sandwichParts[i].position = Vector3.Lerp(sandwichParts[i].position,target + Vector3.up * h + waveDir,24*Time.deltaTime);
            }
            if (eatRate < 0)
            {
                eatRate = 0.1f;
                Eat();
                var part = sandwichParts[0];
                sandwichParts.Remove(part);
                Destroy(part.gameObject);
            }
            else
            {
                eatRate -= Time.deltaTime;
            }
        }
    }
    #endregion




    private void SetPathToPlayer()
    {
        if (!win || playerGetPath) return;
        var dist = Vector3.Distance(transform.position, Sandwich.Instance.transform.position);
        if (dist > 0.3f) return;
        playerGetPath = true;
        Sandwich.Instance.StartJumping();
    }
    private void Dizzy()
    {
       if (!lose) return;
       var targetPosition = new Vector3(transform.position.x,0,transform.position.z);
       transform.position = Vector3.Lerp(transform.position, targetPosition, 6 * Time.deltaTime);
       sayNo.transform.rotation = Quaternion.Lerp(sayNo.transform.rotation,Quaternion.Euler(30,0,0), 6*Time.deltaTime);
       var quaternion = new Quaternion(0, 0, 0, -1);
       quaternion.z = Mathf.Sin(Mathf.PI * Time.time) / 10f;
       quaternion.x = Mathf.Sin(Mathf.PI * Time.time + 20) / 13f;
       transform.rotation = quaternion;
       mesh.material.SetFloat("_Stringht",Mathf.Lerp(mesh.material.GetFloat("_Stringht"),0.5f,6*Time.deltaTime));
    }
    private bool IsBad()
    {
        return badStuffRate > 0.2f;
    }
    private void Done()
    {
        if (IsBad())
        {
            eating = eatingAll = false;
            lose = true;
            sayNo.DOPause();
            GameManager.Instance.EndGame(false);
            return;
        }
        if(path.IsNull())GameManager.Instance.EndGame(true);
        win = true;
        collider.enabled = false;
        GameManager.Instance.GetActivePart().grade.SetGradePoint();
        doTween.DOPlay();
    }
    
}
