using Sirenix.OdinInspector;

public enum AudioType
{
    [LabelText("玩家不能使用")] PlayerCannotUse
  , [LabelText("武器拿起")] TakeUpWeapon
  , [LabelText("武器放下")] TakeDownWeapon
  , [LabelText("消耗品使用成功")] UseConsumablesSuccess
  , [LabelText("消耗品使用失败")] UseConsumablesFail
  , [LabelText("背包")] Bag
  , [LabelText("通用失败")] Fail,
}