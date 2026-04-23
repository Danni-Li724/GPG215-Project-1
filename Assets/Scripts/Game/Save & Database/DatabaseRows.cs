using SQLite4Unity3d;

[Table("Enemy")]
public class EnemyStatsRow
{
    [PrimaryKey, AutoIncrement] public int    id           { get; set; }
    [Unique]                    public string type_key     { get; set; }
    public int   max_health   { get; set; }
    public float speed        { get; set; }
    public float drop_rate    { get; set; }
    public int   hit_vfx_type { get; set; }
}

[Table("Level")]
public class LevelRow
{
    [PrimaryKey, AutoIncrement] public int    id { get; set; }
    public string level_name          { get; set; }
    public int    mileage_goal        { get; set; } 
    public float  spawn_rate          { get; set; } 
    public string boss_notice_text    { get; set; }
    public string arrival_notice_text { get; set; }
    public string dlc_bundle_name     { get; set; } // if null, load base game
}

[Table("Item")]
public class ItemRow
{
    [PrimaryKey, AutoIncrement] public int    id  { get; set; }
    [Unique]                    public string item_key  { get; set; } 
    public float duration              { get; set; }
    public float fire_dps              { get; set; }
    public float fire_spread_radius    { get; set; }
    public float fire_stack_multiplier { get; set; }
}

[Table("LevelNode")]
public class LevelNodeRow
{
    [PrimaryKey, AutoIncrement] public int    id             { get; set; }
    public int    level_id        { get; set; } 
    public string node_type       { get; set; }  
    public int    spawn_distance  { get; set; }  
    public string sprite_key      { get; set; }
    public int    variation_count { get; set; } 
    public float  scale_min       { get; set; }
    public float  scale_max       { get; set; }
    public int    layer           { get; set; }  
}

[Table("PlayerBest")]
public class PlayerBestRow
{
    [PrimaryKey, AutoIncrement] public int    id             { get; set; }
    public int    total_mileage  { get; set; }
    public int    enemies_killed { get; set; }
    public int    lives_lost     { get; set; }
    public string timestamp      { get; set; }
}
