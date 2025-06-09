using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Wall : CustomTileBase
{
    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        if (boxCollider2D != null)
        {
            boxCollider2D.enabled = true;
            boxCollider2D.isTrigger = false;
            boxCollider2D.size = new Vector2( 1, 1 );
        }

        if (!GameTestManager.GetInstance().allMapVisibleMode)
        {
            SetVisibility( false );
        }
    }

    public Wall( Vector2 _posWorld ) : base( _posWorld )
    {
        this.posWorld = _posWorld;
    }
}
