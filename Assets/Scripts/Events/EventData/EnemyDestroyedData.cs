public class EnemyDestroyedData
{
    public Enemy enemy { get; private set; }
    public Tower tower { get; private set; }

    public EnemyDestroyedData(Enemy enemy, Tower tower)
    {
        this.enemy = enemy;
        this.tower = tower;
    }
}
