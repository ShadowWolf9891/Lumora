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

    public void ChangeImage(int index)
    {
        if (index <= sprites.Count() || index >= 0)
        {
            displayImage.sprite = sprites[index];
        }
    }
}
