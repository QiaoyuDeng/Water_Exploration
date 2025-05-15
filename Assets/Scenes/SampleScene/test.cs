using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class DialogTest : MonoBehaviour
{
    public GameObject dialogPrefab;

    void Start()
    {
        //// ✅ 自定义按钮文字
        //DialogButtonContext[] buttons = new DialogButtonContext[]
        //{
        //    new DialogButtonContext("Replay", DialogButtonType.Yes),
        //    new DialogButtonContext("Next", DialogButtonType.No)
        //};

        //Dialog myDialog = Dialog.Open(
        //    dialogPrefab,
        //    DialogButtonType.Yes | DialogButtonType.No,
        //    "Test Dialog",
        //    "This is a test.",
        //    true
        //);

        //myDialog.Result.ButtonContexts = buttons;

        Dialog myDialog = Dialog.Open(
            dialogPrefab
        );

        if (myDialog != null)
        {
            myDialog.OnClosed += result =>
            {
                Debug.Log("Dialog closed: " + result.Result);
            };
        }
        else
        {
            Debug.LogError("Dialog.Open failed!");
        }
    }
}
