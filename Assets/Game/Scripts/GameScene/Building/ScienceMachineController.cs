using Project_WildernessSurvivalGame;

public class ScienceMachineController : BuildingBase
{
    public override void Init(MapChunkController chunk, ulong objectId, bool isFromBuild)
    {
        base.Init(chunk, objectId, isFromBuild);
        if (isFromBuild)
        {
            // 只有第一次建造成功时才同步科技数据
            ScienceManager.Instance.AddScience(27);
        }
    }
}