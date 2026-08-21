using UnityEngine;

public static class MenuButtonManager
{
    private static MenuButton activeButton;

    public static void SetActiveButton(MenuButton newButton)
    {
        // 同じボタンなら何もしない
        if (activeButton == newButton)
            return;

        // 前のボタンを終了
        if (activeButton != null)
        {
            activeButton.StopFloating();
        }

        // 新しいボタンを開始
        activeButton = newButton;
    }
}
