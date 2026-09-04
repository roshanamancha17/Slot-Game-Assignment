using UnityEngine;
using UnityEngine.UI; // Needed to talk to UI Images

public class Reel : MonoBehaviour
{
    public bool isSpinning = false;
    public float spinSpeed = 1500f;
    
    private float resetPosition = -400f; 
    private float startPosition = 400f;

    private Vector2[] startPositions = new Vector2[5]; // Remembers original layout
    private Image middleSymbol; // The 3rd picture in the column

    void Start()
    {
        // Memorize where all 5 symbols started so we can snap back perfectly
        for (int i = 0; i < transform.childCount; i++)
        {
            startPositions[i] = transform.GetChild(i).localPosition;
            
            // The 3rd symbol (Index 2) is our middle one!
            if (i == 2) middleSymbol = transform.GetChild(i).GetComponent<Image>();
        }
    }

    void Update()
    {
        if (!isSpinning) return;

        foreach (Transform symbol in transform)
        {
            symbol.Translate(Vector2.down * spinSpeed * Time.deltaTime);
            if (symbol.localPosition.y <= resetPosition)
            {
                symbol.localPosition = new Vector2(symbol.localPosition.x, startPosition);
            }
        }
    }

    public void StartSpinning()
    {
        isSpinning = true;
    }

    // NEW: Stop the reel and force it to show our RNG result
    public void StopAt(Sprite resultSprite)
    {
        isSpinning = false;
        
        // Swap the middle picture to our randomly chosen winning artwork
        if (middleSymbol != null)
        {
            middleSymbol.sprite = resultSprite;
        }

        // Snap all symbols perfectly back into their original starting grid
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).localPosition = startPositions[i];
        }
    }
}