using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;

namespace Managers
{

    public class PetSceneManager : MonoBehaviour
    {
        [Header("Scene Objects")]

        public GameObject warningUI;
        public TMP_Text warningText;
        public Slider hungerBar;
        public Slider expBar;

        //Pet Datas
        int petID;
        string petName;
        int food;
        float hungerStats;
        float expStats;
        double lastFedTime;
        double currentTime;

        bool needsUpdate;

        // Start is called before the first frame update
        void Start()
        {
            needsUpdate = false;
            StartCoroutine(start_coroutine());
        }

        private IEnumerator start_coroutine()
        {
            FirebaseManager.instance.check_pet_data_exist();

            while (!FirebaseManager.instance.isDataRecieved())
            {
                yield return null;
            }

            if (!FirebaseManager.instance.get_pet_data_exist())
            {
                FirebaseManager.instance.initializePetData();

                while (!FirebaseManager.instance.isDataRecieved())
                {
                    yield return null;
                }
            }

            FirebaseManager.instance.readPetData();

            while (!FirebaseManager.instance.isDataRecieved())
            {
                yield return null;
            }

            petID = int.Parse(PlayerManager.instance.getData("petID"));
            petName = FirebaseManager.instance.getPetName();
            food = FirebaseManager.instance.getFood();
            hungerStats = FirebaseManager.instance.getHungerStats();
            expStats = FirebaseManager.instance.getExpStats();
            lastFedTime = System.Math.Round(FirebaseManager.instance.getLastFedTime(), 4);
            currentTime = System.Math.Round((double)System.DateTime.Now.Ticks / (36000000000 * 24), 4);

            if (petID != 0)
            {
                if (isPetDead())
                {
                    displayWarning("Your pet has died of starvation.");
                }
                else
                {
                    needsUpdate = true;
                }
            }
            else
            {
                hungerBar.value = 0;
                displayWarning("You do not own a pet yet, Go to the pet shop to buy one.");
            }



            LevelLoader.instance.ClearCrossFade();
        }

        void displayWarning(string message)
        {
            warningText.text = message;
            warningUI.SetActive(true);
        }

        public void warningUI_Button()
        {
            LevelLoader.instance.loadScene("Lobby");
        }

        bool isPetDead()
        {
            if (currentTime - lastFedTime > 7)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (needsUpdate)
            {
                StartCoroutine(update_coroutine());
            }

        }

        private IEnumerator update_coroutine()
        {
            needsUpdate = false;

            yield return new WaitForEndOfFrame();

            FirebaseManager.instance.readPetData();

            while (!FirebaseManager.instance.isDataRecieved())
            {
                yield return null;
            }

            petName = FirebaseManager.instance.getPetName();
            food = FirebaseManager.instance.getFood();
            hungerStats = FirebaseManager.instance.getHungerStats();
            expStats = FirebaseManager.instance.getExpStats();
            lastFedTime = System.Math.Round(FirebaseManager.instance.getLastFedTime(), 4);
            currentTime = System.Math.Round((double)System.DateTime.Now.Ticks / (36000000000 * 24), 4);

            float hungerStatsProgressBar = hungerStats / 70;
            hungerBar.value = hungerStatsProgressBar;

            needsUpdate = true;
        }


        //UI Buttons
        public void leave_button()
        {
            LevelLoader.instance.loadScene("Lobby");
        }

        public void feed()
        {
            int stomcahSpace = 70 - (int)hungerStats;

            if (food >= stomcahSpace)
            {
                FirebaseManager.instance.writeFood(food - stomcahSpace);
                FirebaseManager.instance.writeHungerStats(70);
            }
            else
            {
                FirebaseManager.instance.writeFood(0);
                FirebaseManager.instance.writeHungerStats(hungerStats + food);
            }
        }
    }
}
