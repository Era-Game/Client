using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.ModernUIPack;

namespace Managers
{
    public class OrganizationMenuManager : MonoBehaviour
    {
        public TMP_Dropdown CompanySelection;
        public TMP_Dropdown DepartmentSelection;

        public CustomToggle HeadToggle;

        private bool isHead;
        private int companyIndex;
        private int departmentIndex;
        private string selectedCompanyName;
        private string selectedDepartmentName;
        private string selectedDepartmentID;
        private List<string> CompanyList;
        private List<string> DepartmentList;
        private List<string> DepartmentIDList;

        // Start is called before the first frame update
        void Start()
        {
            isHead = true;
            companyIndex = 0;
            departmentIndex = 0;
            CompanyList = new List<string>();
            DepartmentList = new List<string>();
            CompanySelection.options.Clear();
            DepartmentSelection.options.Clear();

            StartCoroutine(start_coroutine());

            CompanySelection.onValueChanged.AddListener(delegate { company_dropdown_event(CompanySelection); });
            DepartmentSelection.onValueChanged.AddListener(delegate { department_dropdown_event(DepartmentSelection); });

            LevelLoader.instance.ClearCrossFade();
        }

        private IEnumerator start_coroutine()
        {
            FirebaseManager.instance.readCompanyNames();
            yield return new WaitForSeconds(0.5f);

            while (FirebaseManager.instance.getCompanyNameList() == null)
            {
                Debug.Log("Getting Company List From Firebase...");
                FirebaseManager.instance.readCompanyNames();
                yield return null;
            }

            CompanyList = FirebaseManager.instance.getCompanyNameList();
            CompanyList.Insert(0, "--Select a Company--");

            foreach (var item in CompanyList)
            {
                CompanySelection.options.Add(new TMP_Dropdown.OptionData() { text = item });
            }

            selectedCompanyName = CompanySelection.options[1].text;
            StartCoroutine(update_department_list());

        }

        public void company_dropdown_event(TMP_Dropdown dropdown)
        {
            companyIndex = dropdown.value;
            Debug.Log("Company Dowpdown Value (" + companyIndex + "): " + dropdown.options[companyIndex].text);
            selectedCompanyName = dropdown.options[companyIndex].text;

            StartCoroutine(update_department_list());
        }

        IEnumerator update_department_list()
        {
            DepartmentSelection.options.Clear();

            FirebaseManager.instance.readDepartmentNames(selectedCompanyName);
            yield return new WaitForSeconds(0.5f);

            while (FirebaseManager.instance.getDepartmentList() == null || FirebaseManager.instance.getDepartmentList().Count == 0)
            {
                FirebaseManager.instance.readDepartmentNames(selectedCompanyName);
                yield return null;
            }

            DepartmentIDList = new List<string>();
            DepartmentList = new List<string>();

            DepartmentList = FirebaseManager.instance.getDepartmentList();
            DepartmentIDList = FirebaseManager.instance.getDepartmentIDList();

            DepartmentList.Insert(0, "--Select a Department--");
            DepartmentIDList.Insert(0, "---Ignore---");
            foreach (var item in DepartmentList)
            {
                DepartmentSelection.options.Add(new TMP_Dropdown.OptionData() { text = item });
            }
        }

        public void department_dropdown_event(TMP_Dropdown dropdown)
        {
            departmentIndex = dropdown.value;
            Debug.Log("Company Dowpdown Value (" + departmentIndex + "): " + dropdown.options[departmentIndex].text);
            selectedDepartmentName = dropdown.options[departmentIndex].text;
            selectedDepartmentID = DepartmentIDList[departmentIndex];
        }

        public void toggle()
        {
            if (isHead)
            {
                isHead = false;
            }
            else
            {
                isHead = true;
            }
        }

        public void enter_selection()
        {
            //Call API
            Debug.Log("Selection Result: " + "Company Name - " + selectedCompanyName + "; Department - " + selectedDepartmentName + "; Department ID - " + selectedDepartmentID + "; Department Head - " + isHead.ToString());

            if (selectedCompanyName != "--Select a Company--" && selectedDepartmentName != "--Select a Department--")
            {
                StartCoroutine(enter_selection_coroutine());
            }
            else
            {
                Debug.Log("Invalid Selection");
            }

        }

        private IEnumerator enter_selection_coroutine()
        {
            API.instance.Post_Org_Request(selectedDepartmentID, isHead, PlayerManager.instance.getData("uid"));

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            long responseStatus = API.instance.statusCode;
            string responseMessage = API.instance.responseMessage;

            if (responseStatus == 200)
            {
                LevelLoader.instance.loadScene("Lobby");
            }
        }

        private void Update()
        {
            //Debug.Log("Selection Result: " + "Company Name - " + selectedCompanyName + "; Department - " + selectedDepartmentName + "; Department Head - " + isHead.ToString());
        }
    }

}
