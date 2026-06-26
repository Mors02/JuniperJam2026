using System;
using System.Collections.Generic;
using UnityEngine;

public class GondolaScript : MonoBehaviour
{
    [SerializeField] public GameObject anchorPoint;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rigidbody2D;
    [SerializeField] private Collider2D col;
    [SerializeField] private bool RandomizeColor = true;
    [SerializeField] private List<Sprite> possibleColors;
    public Vector2 previousPos; 
    public Vector2 Velocity; 
    public bool usedByCharacter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int i = UnityEngine.Random.Range(0,possibleColors.Count - 1);
       
        spriteRenderer.sprite = possibleColors[i];
        previousPos = rigidbody2D.position;
        col.isTrigger = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        col.isTrigger = !usedByCharacter;
        rigidbody2D.MovePosition(anchorPoint.transform.position);
        Velocity = (rigidbody2D.position - previousPos) / Time.deltaTime;
        //print(rigidbody2D.position + " " + previousPos);
       //this.transform.eulerAngles = new Vector3(0,0,0);
       previousPos = rigidbody2D.position;
    }
}
