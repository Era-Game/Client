using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class SkinShopManager : MonoBehaviour
{

    [Header("Skin Shop Utilities")]
    public Text coinText;
    public Text animation_State_Text;
    public Text select_button_Text;
    public ButtonManager select_button;
    public Text price_tag;
    public Transform spawnPoint;

    private int skinID = 0;
    private int animation_State = 0;
    private double[] skin_prices = { 0, 0, 3000, 3000, 10000, 10000, 20000, 30000, 30000};
    private string[] animation_State_Texts = { "Idle", "Walking", "Running", "Victory Dance"};
    private GameObject currentClone;
    private Animator animator;

    private readonly int SKIN_NUM = 9;
    private readonly int ANIMATION_NUM = 4;

    private void Start()
    {
        currentClone = Instantiate(SkinManager.instance.getSkinByID(0), spawnPoint.position, Quaternion.identity);
        currentClone.transform.parent = spawnPoint;
        currentClone.transform.localScale = new Vector3(11.0f, 11.0f, 11.0f);
        currentClone.transform.localPosition = new Vector3(0, -2.56f, 0);
        currentClone.transform.Rotate(0, 90, 0);
        currentClone.GetComponent<PlaneTexture>().hideNameTag();
        animator = currentClone.GetComponent<Animator>();
        animator.SetInteger("state", 0);
        animation_State_Text.text = animation_State_Texts[0];
        LevelLoader.instance.ClearCrossFade();
    }

    public void change_skin_next()
    {
        Destroy(currentClone);

        skinID = (skinID + 1) % SKIN_NUM;

        currentClone = Instantiate(SkinManager.instance.getSkinByID(skinID), spawnPoint.position, Quaternion.identity);
        currentClone.transform.parent = spawnPoint;
        currentClone.transform.localScale = new Vector3(11.0f, 11.0f, 11.0f);
        currentClone.transform.localPosition = new Vector3(0, -2.56f, 0);
        currentClone.transform.Rotate(0, 90, 0);
        currentClone.GetComponent<PlaneTexture>().hideNameTag();
        animator = currentClone.GetComponent<Animator>();
    }

    public void change_skin_prev()
    {

        Destroy(currentClone);


        if (skinID == 0)
        {
            skinID = SKIN_NUM - 1;
        }
        else
        {
            skinID -= 1;
        }

        currentClone = Instantiate(SkinManager.instance.getSkinByID(skinID), spawnPoint.position, Quaternion.identity);
        currentClone.transform.parent = spawnPoint;
        currentClone.transform.localScale = new Vector3(11.0f, 11.0f, 11.0f);
        currentClone.transform.localPosition = new Vector3(0, -2.56f, 0);
        currentClone.transform.Rotate(0, 90, 0);
        currentClone.GetComponent<PlaneTexture>().hideNameTag();
        animator = currentClone.GetComponent<Animator>();
    }

    public void change_animator_state()
    {
        animation_State = (animation_State + 1) % ANIMATION_NUM;
        animation_State_Text.text = animation_State_Texts[animation_State];
    }


    public void buySkin()
    {
        bool logic = PlayerManager.instance.ownSkin(skinID);
        Debug.Log("Buy Skin Logic: " + logic.ToString());
        if (PlayerManager.instance.ownSkin(skinID))
        {
            PlayerManager.instance.setSkinID(skinID.ToString());
            select_button.buttonText = "Selected";
            select_button.UpdateUI();
        }
        else if(!PlayerManager.instance.ownSkin(skinID) && int.Parse(PlayerManager.instance.getData("coins")) >= skin_prices[skinID])
        {
            Debug.Log("New Skin Acquired");
            PlayerManager.instance.addCoins("-" + skin_prices[skinID].ToString());
            PlayerManager.instance.setSkinID(skinID.ToString());
            PlayerManager.instance.buySkin(skinID);
            select_button_Text.text = "Select";
            select_button.buttonText = "Selected";
            select_button.UpdateUI();
        }
        
    }

    public void exitButton()
    {
        LevelLoader.instance.loadScene("Lobby");
    }

    // Update is called once per frame
    void Update()
    {
        coinText.text = PlayerManager.instance.getData("coins");
        animator.SetInteger("state", animation_State);
        if (PlayerManager.instance.ownSkin(skinID))
        {
            price_tag.text = "OWNED";
            if (skinID.ToString() == PlayerManager.instance.getData("skinID"))
            {
                select_button.buttonText = "Selected";
                select_button.UpdateUI();
            }
            else
            {
                select_button_Text.text = "Select";
                select_button.buttonText = "Select";
                select_button.UpdateUI();
            }
            
        }
        else
        {
            if (skin_prices[skinID] == 0)
            {
                price_tag.text = "FREE";
            }
            else
            {
                price_tag.text = "$ " + skin_prices[skinID].ToString();
            }
            
            select_button_Text.text = "Buy";
            select_button.buttonText = "Buy";
            select_button.UpdateUI();
        }

        
    }
}
