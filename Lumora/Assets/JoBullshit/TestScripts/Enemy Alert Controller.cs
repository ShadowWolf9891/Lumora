using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAlertController : MonoBehaviour
{
    Image displayImage;

    [Header("Image Bank")]
    [SerializeField]
    Sprite[] sprites;

    private void Start()
    {
        displayImage = GetComponent<Image>();
    }

    public void ChangeImage(AlertStates state)
    {
        if ((int)state <= sprites.Count() || (int)state >= 0)
        {
            displayImage.sprite = sprites[(int)state];
        }
    }
}

public enum AlertStates
{ 
    IDLE = 0,
    ALERT = 1,
    CHASING = 2
    

}

