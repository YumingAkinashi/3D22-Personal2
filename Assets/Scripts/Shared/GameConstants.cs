using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConstants : MonoBehaviour
{
    // 所有常用的字串會被儲存成常數，比較方便用
    // const修飾詞類似static，只是初始化後無法修改

    // 座標名
    public const string k_AxisNameVertical = "Vertical";
    public const string k_AxisNameHorizontal = "Horizontal";

    // 滑鼠座標位置
    public const string k_MouseAxisNameVertical = "Mouse Y";
    public const string k_MouseAxisNameHorizontal = "Mouse X";

    // 遊戲動作字串
    public const string k_ButtonNameAim = "Aim";
    public const string k_ButtonNameFire = "Fire";
    public const string k_ButtonNameSprint = "Sprint";
    public const string k_ButtonNameJump = "Jump";

    // 不確定會不會用到
    public const string k_ButtonNamePauseMenu = "Pause Menu";
    public const string k_ButtonNameCancel = "Cancel";
    public const string k_ButtonReload = "Reload";
}
