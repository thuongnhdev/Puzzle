using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create Config Variables")]
public class ConfigVariables : ScriptableObject
{
    public float DISTANCE_FILL_TARGET;
    public float PERCENT_FILL_TARGET = 0.1f;
    public float DISTANCE_TO_DRAG_OBJECT = 0.05f;
    public float DISTANCE_FINGER;
    public float TIME_HIDE_OBJ_GUIDLINE;
    public float TIME_SHOW_OBJ_GUIDLINE;
    public float TIME_FILL_OBJECT;
    public float TIME_NEXT_LAYER;
    public float SCALE_OBJECT_TO = 1.2f;
    public float SCALE_OBJECT_BACK = 1.0f;
    public float DURATION_SCALE_OBJECT = 0.2f;
    public float TIME_FADDING_POPUP = 0.5f;
    public float TIME_UPDATE_PROGRESS_BAR = 0.5f;
    public int NUMBER_TUTORIAL_STEP = 3;
    public float TIME_COOLDOWN_SHOW_TUTORIAL = 2.0f;
    public float SIZE_DRAG_WITH_TARGET_OBJ = 2.0f;
    public float DELAY_FILL_TARGET = 0.03f;
    public int MAX_RESUME_PLAYING_PUZZLE = 7;
    public int MAX_LATEST_UPDATE_PUZZLE = 10;
    public float MIN_LOADING_WAITING_TIME = 2.0f;
    public float ZOOM_SPEED = 1.0f;
    public int MAX_FREE_PUZZLE = 5;
    public int MAX_BOOK_SIZE = 23;



    public List<string> HINT_TEXT = new List<string>();
    public List<int> DailyInkReward = new List<int> {50, 100, 150, 200, 300};
    public Vector3[] ArrScale;
}
