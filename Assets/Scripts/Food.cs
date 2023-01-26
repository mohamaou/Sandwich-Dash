using System;
using UnityEngine;
using Random = UnityEngine.Random;


public enum FoodType
{
    Good, Bad
}

public class Food : MonoBehaviour
{
    public FoodType foodType;
    private bool inPlayer;
    [SerializeField] private LayerMask sandwichLayer;
    [SerializeField] [Range(0,0.5f)] private float radius = 1f;
    [SerializeField] private MeshFilter filter;
    [SerializeField] private Mesh[] goodFood, badFood;
    [SerializeField] private GameObject smokeEffect;


    private void Start()
    {
        if(smokeEffect != null)smokeEffect.SetActive(foodType == FoodType.Bad);
    }

    public void SetRandomFood()
    {
        if (foodType == FoodType.Good)
        {
            var randomValue = Random.Range(0, goodFood.Length);
            for (int i = 0; i < goodFood.Length; i++)
            {
                if (i == randomValue) filter.mesh = goodFood[i];
            }
        }
        else
        {
            var randomValue = Random.Range(0, badFood.Length);
            for (int i = 0; i < badFood.Length; i++)
            {
                if (i == randomValue) filter.mesh = badFood[i];
            } 
        }

        if (transform.childCount > 0)
        {
            transform.rotation = Quaternion.identity;
            transform.GetChild(0).eulerAngles = new Vector3(transform.eulerAngles.x, Random.Range(0,360), transform.eulerAngles.z);
        }
    }
   
    private void Update()
    {
        IsPlayerEnter();
    }

    private void IsPlayerEnter()
    {
        if (inPlayer) return;
        var results = new Collider[4];
        var size = Physics.OverlapSphereNonAlloc(transform.position, radius, results, sandwichLayer);
        if(size == 0) return;
        inPlayer = true;
        Sandwich.Instance.AddSandwichPart(this);
    }
}
