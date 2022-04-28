using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Candy : MonoBehaviour
{
    private int x;
    public int X { 
      get { return x; }
        set
        {
            if (IsMovable())
                x = value;
        }
    }
    private int y;
    public int Y {
        get { return y; }
        set
        {
            if (IsMovable())
                y = value;
        }
    }

    private Grid grid;

    private MovablePiece movableComponent;
    public MovablePiece MovableComponent
    {
        get { return movableComponent; }
    }

    private CandyColor colorComponent;
    public CandyColor ColorComponent
    {
        get { return colorComponent; }
    }

    private ClearablePiece clearableComponent;
    public ClearablePiece ClearableComponent
    {
        get { return clearableComponent; }
    }

    public Grid gridRef
    {
        get { return grid; }
    }
    private Grid.CandyType type;
    public Grid.CandyType Type
    {
        get { return type; }
    }
    private void Awake()
    {
        movableComponent = GetComponent<MovablePiece>();
        colorComponent = GetComponent<CandyColor>();
        clearableComponent = GetComponent<ClearablePiece>();
    }

    public void Init(int x, int y, Grid grid, Grid.CandyType type)
    {
        this.x = x;
        this.y = y;
        this.grid = grid;
        this.type = type;
    }

    public bool IsMovable()
    {
        return movableComponent != null;
    }

    public bool IsColored()
    {
        return colorComponent != null;
    }

    public bool IsClearable()
    {
        return clearableComponent != null;
    }

    private void OnMouseDown()
    {
        if (!grid.IsDestroying &&
            !GameManager.instance.gameOver &&
            !GameManager.instance.gamePause)
        {
            grid.PressCandy(this);
            SFXManager.instance.PlaySFX(Clip.Select);
        }
    }
    private void OnMouseEnter()
    {
        if (!grid.IsDestroying &&
            !GameManager.instance.gameOver &&
            !GameManager.instance.gamePause) { 
            grid.EnterCandy(this);
        }
    }
    private void OnMouseUp()
    {
        if (!grid.IsDestroying &&
            !GameManager.instance.gameOver &&
            !GameManager.instance.gamePause)
            grid.ReleaseCandy();
    }

    public void CantSwap()
    {
        Animator anim = GetComponent<Animator>();

        if (anim)
        {
            anim.SetTrigger("UnSwappable");
        }
    }
}
