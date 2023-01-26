using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Sandwich : MonoBehaviour
{
    public static Sandwich Instance { get; private set; }
    [SerializeField] private LayerMask wallLayer, headLayer;
    [SerializeField] private float speed = 6f, jumpHeight = 3.5f;
    [Range(0, 0.4f)] public float height = 0.3f;
    [SerializeField] private Transform cameraPoint;
    private Direction waveDirection;
    private float wave, offesetTime, waveHeight;
    private bool moving, headFull, inHead, jumping, startJumping, betweenPlatrorms;
    private Vector3 cameraTargetPoint;

    //Food
    private List<Food> sandwichParts = new List<Food>();
    private List<Transform> sandwichPartsReves = new List<Transform>();
    private List<Vector3> sandwichPartsJumpPoint = new List<Vector3>();

    //Jump
    private Vector3 jumpPoint;
    private float distanceTravelled;


    private void Awake()
    {
        Instance = this;
        if (transform.childCount >= 1)
        {
            sandwichParts.Add(transform.GetChild(0).GetComponent<Food>());
            transform.GetChild(0).SetParent(null);
        }
    }
    
    private void Update()
    {
        if (GameManager.State != GameState.Play) return;
        SetSandwichPartsPosition(); 
        IsInHead(); 
        SetActiveParts();
        Jumping();
    }

    private void LateUpdate()
    {
        CameraMovement();
    }


    #region Eating

    private bool IsInHead()
    {
        var results = new Collider[10]; 
        var size = Physics.OverlapSphereNonAlloc(transform.position, 0.25f, results, headLayer);
        if (size == 0) return false;
        if (inHead) return true;
        inHead = true; 
        for (int i = 0; i < results.Length; i++) 
        { 
            if (results[i] != null)
            {
                var head = results[i].GetComponent<Head>();
                if (head.full) return false;
                var targetPoint = head.transform.position - head.transform.forward;
                if (head.height < sandwichParts.Count)
                {
                    moving = false;
                    headFull = true;
                    targetPoint.y = transform.position.y;
                    SetMovementDirection(Direction.Null,GameManager.Instance.GetActivePart().grade.GetClosestPoint(targetPoint));
                    var lowerParts = new List<Transform>();
                    for (int j = 0; j < head.height; j++)
                    {
                        var part = sandwichParts[i];
                        sandwichParts.Remove(part);
                        lowerParts.Add(part.transform);
                    }
                    head.EatAll(lowerParts);
                    return true;
                }
                StartCoroutine(EatMe(head));
            }
        }
        return true;
    }
    
    private IEnumerator EatMe(Head head)
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (sandwichParts.Count <= 1  || ! IsInHead() || head.Eat())
            {
                inHead = false;
                break;
            }
            var part = sandwichParts[0];
            sandwichParts.Remove(part);
            Destroy(part.gameObject);
        }
    }

    #endregion
    
    #region Movement
    public void SetMovementDirection(Direction direction, Vector3 autoTarget = new Vector3())
    {
        if(GameManager.State == GameState.Start)GameManager.Instance.Play();
        if (moving || betweenPlatrorms) return;
        var move = true;
        Vector3 target;
        if (direction == Direction.Null)
        {
            target = autoTarget;
        }
        else
        {
            target = GetTargetPoint(direction, out move);
        }
        if (!move) return;
        StartCoroutine(Move(target, direction != Direction.Null));
        moving = true;
        waveDirection = direction;
        
    }
    private IEnumerator Move(Vector3 targetPoint, bool setOtherPart)
    {
        while (true)
        {
            yield return new WaitForSeconds(0);
            var position = transform.position;
            position = Vector3.MoveTowards(position,targetPoint,speed * Time.deltaTime);
            transform.position = position;
            var dist = Vector3.Distance(position, targetPoint);
            if (dist < 0.1 || headFull && setOtherPart || jumping)
            {
                if (headFull && setOtherPart) headFull = false;
                SandwichReachWall();
                break;
            }
        }
    }
    private Vector3 GetTargetPoint(Direction direction, out bool move)
    {
        var dir = new Vector3();
        var position = transform.position;
        switch (direction)
        {
            case Direction.Forward: dir = Vector3.forward;
                break;
            case Direction.Back: dir = Vector3.back;
                break;
            case Direction.Right: dir = Vector3.right;
                break;
            case Direction.Left: dir = Vector3.left;
                break;
        }
        var hitPoint = position;
        if (Physics.Raycast(transform.position + Vector3.up * 0.25f, dir, out var hit, Mathf.Infinity, wallLayer))
        {
            var dist = Vector3.Distance(new Vector3(hit.point.x, 0, hit.point.z),new Vector3(position.x, 0, position.z));
            if (dist > 0.9f)
            {
                hitPoint = hit.point;
                move = true;
            }
        }
        hitPoint.y = position.y;
        hitPoint = GameManager.Instance.GetActivePart().grade.GetClosestPoint(hitPoint);
        move = Vector3.Distance(position,hitPoint) >= 0.2f;
        return hitPoint;
    }
    private void SandwichReachWall()
    {
        offesetTime = 0;
        wave = 2;
        moving = false;
    }
    #endregion

    #region Stack
    public void AddSandwichPart(Food part)
    {
        waveHeight = 0.05f;
        var array = sandwichParts.ToArray();
        sandwichParts.Clear();
        sandwichParts.Add(part);
        for (int i = 0; i < array.Length; i++)
        {
            sandwichParts.Add(array[i]);
        }
    }
    private void SetSandwichPartsPosition()
    {
        if (jumping) return;
        wave = Mathf.Lerp(wave, 0,3.5f * Time.deltaTime);
        waveHeight = Mathf.Lerp(waveHeight, 0, 3.5f * Time.deltaTime);
        offesetTime += Time.deltaTime *2;
        var stackSpeed = 32f;
        if (betweenPlatrorms) stackSpeed = 12f;
        var w = wave * Mathf.Sin(Mathf.PI * offesetTime);
        for (int i = 0; i < sandwichParts.Count; i++)
        {
            var target = i == 0 ? transform.position : sandwichParts[i-1].transform.position;
            var waveDir = new Vector3();
            switch (waveDirection)
            {
                case Direction.Forward: waveDir = Vector3.forward * i/3 * 0.03f * w;
                    break;
                case Direction.Back:waveDir = Vector3.back * i/3 * 0.03f * w;
                    break;
                case Direction.Right:waveDir = Vector3.right * i/3 * 0.03f * w;
                    break;
                case Direction.Left:waveDir = Vector3.left * i/3 * 0.03f * w;
                    break;
            }
            target.y = sandwichParts[i].transform.position.y;
            if (!betweenPlatrorms)
            {
                sandwichParts[i].transform.position = Vector3.Lerp(sandwichParts[i].transform.position,target + waveDir,stackSpeed*Time.deltaTime);
            }
            else
            {
                sandwichParts[i].transform.position = Vector3.MoveTowards(sandwichParts[i].transform.position,target + waveDir,stackSpeed*Time.deltaTime);

            }
            sandwichParts[i].transform.rotation = Quaternion.Lerp(sandwichParts[i].transform.rotation,Quaternion.LookRotation(Vector3.forward), 6*Time.deltaTime);
        }
        //Height
        for (int i = 0; i < sandwichParts.Count; i++)
        {
            var target = sandwichParts[i].transform.position; 
            target.y = i == 0 ? transform.position.y : sandwichParts[i-1].transform.position.y;
            var h = height + waveHeight;
            if (i == 0) h = 0;
            sandwichParts[i].transform.position = Vector3.MoveTowards(sandwichParts[i].transform.position,target + Vector3.up * h,12 * Time.deltaTime);
        }
    }
    private void SetActiveParts()
    {
        var goodOrBad = new List<bool>();
        for (int i = 0; i < sandwichParts.Count; i++)
        {
            if (GameManager.Instance.GetActivePart().grade.head.height > i)
            {
                goodOrBad.Add(sandwichParts[i].foodType == FoodType.Good);
            }
        }
        var isGood = goodOrBad.Count;
        for (int i = 0; i < goodOrBad.Count; i++)
        {
            if (goodOrBad[i]) isGood--;
        }
        GameManager.Instance.GetActivePart().grade.head.SetBadStuffRate((float)isGood/goodOrBad.Count); 
    }
    #endregion

    #region Jumping

    private bool IsStackStable()
    {
        if (sandwichParts.Count < 2) return true;
        var up = new Vector3(sandwichParts[sandwichParts.Count - 1].transform.position.x,0,sandwichParts[sandwichParts.Count - 1].transform.position.z);
        var down = new Vector3(sandwichParts[sandwichParts.Count - 2].transform.position.x,0,sandwichParts[sandwichParts.Count - 2].transform.position.z);
        var dist = Vector3.Distance(up,down);
        return  wave * Mathf.Sin(Mathf.PI * offesetTime) >= 0 && dist < 0.1f && wave < 0.2f;
    }
    
    public void StartJumping()
    {
        betweenPlatrorms = true;
        startJumping = true;
        inHead = false;
    }

    private void EndJump()
    {
        startJumping = false;
        jumping = false;
        betweenPlatrorms = false;
        GameManager.Instance.partIndex++;
        SetCameraToPlatform();
    }
    
    private void NextJump()
    {
        if (jumping) return;
        startJumping = false;
        jumping = true;
        distanceTravelled = 0;
        bool endJump;
        var point = GameManager.Instance.GetActivePart().grade.head.path.GetJupPoint(out endJump);
        if (endJump)
        {
            EndJump();
            return;
        }
        jumpPoint = point;
        transform.position = jumpPoint;
        sandwichPartsReves.Clear();
        sandwichPartsJumpPoint.Clear();
        sandwichParts.Reverse();
        for (int i = 0; i < sandwichParts.Count; i++)
        {
            sandwichPartsReves.Add(sandwichParts[i].transform);
            sandwichPartsJumpPoint.Add(sandwichParts[i].transform.position);
        }
    }

    private void Jumping()
    {
        if(startJumping && IsStackStable())
        {
            NextJump();
        }
        if (!jumping) return;
        transform.position = jumpPoint;
        distanceTravelled += Time.deltaTime * speed / 3;
        for (int i = 0; i < sandwichPartsReves.Count; i++)
        {
            var startPoint = sandwichPartsJumpPoint[i];
            var rotationTarget = GetJumpCurveBetweenTowPoints(distanceTravelled - (i + 4) * height, startPoint, jumpPoint);
            var movementTarget = GetJumpCurveBetweenTowPoints(distanceTravelled - i * height, startPoint, jumpPoint);
            sandwichPartsReves[i].position = movementTarget;
            if (rotationTarget != movementTarget)
            {
                sandwichPartsReves[i].LookAt(rotationTarget); 
                sandwichPartsReves[i].eulerAngles += new Vector3(90, 0, 0);
            }
        }
    }
    
    private void JumpDone()
    {
        if(!jumping)return;
        jumping = false;
        StartCoroutine(PrepareToJump());
    }
    
    private IEnumerator PrepareToJump()
    {
        while (true)
        {
            yield return new WaitForSeconds(0);
            if (IsStackStable())
            {
                NextJump();
                break;
            }
        }
    }

    private Vector3 GetJumpCurveBetweenTowPoints(float offset, Vector3 startPoint, Vector3 target)
    {
        if (offset < 0) return startPoint;
        float curveHeight = jumpHeight;
        curveHeight += startPoint.y / 2;
        var distance = Vector3.Distance(startPoint, target);
        if (offset > distance)
        {
            JumpDone();
            return target;
        }
        var h = Mathf.Sin(offset / distance * Mathf.PI) * curveHeight;
        return Vector3.MoveTowards(startPoint, target, offset) + Vector3.up * h;
    }

    #endregion

    #region Camera

    private void CameraMovement()
    {
        if (betweenPlatrorms)
        {
            cameraPoint.position = Vector3.MoveTowards(cameraPoint.position,cameraTargetPoint,5f * Time.deltaTime);
        }
        else
        {
            cameraPoint.position = Vector3.Lerp(cameraPoint.position,cameraTargetPoint,6f * Time.deltaTime);
        }
        
        if (!jumping) return;
        var targetPoint = new Vector3(sandwichParts[0].transform.position.x, cameraTargetPoint.y,
            sandwichParts[0].transform.position.z);
        if (cameraTargetPoint.z > sandwichParts[0].transform.position.z) return;
        cameraTargetPoint = targetPoint;
    }
    
    private void SetCameraToPlatform()
    {
       cameraTargetPoint = GameManager.Instance.GetActivePart().grade.transform.position;
    }
    
    #endregion
}
