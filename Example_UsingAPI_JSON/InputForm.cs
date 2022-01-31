using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net.Sockets;
using System.IO;
using System.Xml.Linq;

namespace ITAM_2._0
{
    public partial class Form1 : Form
    {
        private static readonly string POST_URI = @"https://apigtwb2c.us.dell.com/auth/oauth/v2/token";
        private static readonly string GET_URI_header = @"https://apigtwb2c.us.dell.com/PROD/sbil/eapi/v5/assets";
        private static readonly string GET_URI_warranty = @"https://apigtwb2c.us.dell.com/PROD/sbil/eapi/v5/asset-entitlements";
        private static readonly string grant_type = "client_credentials";
        private static readonly string client_id = "...";
        private static readonly string client_secret = "...";
        private static readonly string content_type = "application/x-www-form-urlencoded";

        private const string serverIP = "...";
        private const Int32 port = ...;

        private string computerPurchasedDate = "";
        private string computerHDSerial = @"N/A";
        private string computerModel = "";
        

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblComputerWarranty1.Visible = false;
            lblComputerWarranty2.Visible = false;
            lblComputerWarranty3.Visible = false;
            tbComputerWarranty1.Visible = false;
            tbComputerWarranty2.Visible = false;
            tbComputerWarranty3.Visible = false;
            cbComputerLocation.SelectedIndex = 0;
            tbDebugServiceTag.Text = "...";

            if (DateTime.Today.ToString("d").Length < 10)
            {
                tbComputerTodaysDate.Text = "0";
                tbComputerTodaysDate.Text += DateTime.Today.ToString("d");
            }
            else
            {
                tbComputerTodaysDate.Text = DateTime.Today.ToString("d");
            }
        }

        private void btnDebugDellLookup_Click(object sender, EventArgs e)
        {
            
            rtbDebug.Text = GetDellAssetEntitlement(tbDebugServiceTag.Text.ToUpper()).ToString();
        }

        protected string GetDellAccessToken()
        {
            var client_bearer = new RestClient(POST_URI);

            var request_bearer = new RestRequest(Method.POST);
            request_bearer.AddHeader("cache-control", "no-cache");
            request_bearer.AddHeader("content-type", content_type);
            request_bearer.AddParameter(content_type, "grant_type=" + grant_type + "&client_id=" + client_id + "&client_secret=" + client_secret, ParameterType.RequestBody);

            IRestResponse response_bearer = client_bearer.Execute(request_bearer);

            JObject json_AccessToken = JObject.Parse(response_bearer.Content.ToString());
            string accessToken = json_AccessToken["access_token"].ToString();
            return accessToken;
        }

        protected JObject GetDellAssetEntitlement(string serviceTag)
        {
            var client_assetEntitlement = new RestClient(GET_URI_warranty);

            var request_assetEntitlement = new RestRequest(Method.GET);
            request_assetEntitlement.AddHeader("authorization", "Bearer " + GetDellAccessToken());
            request_assetEntitlement.AddParameter("servicetags", serviceTag);

            IRestResponse response_assetEntitlement = client_assetEntitlement.Execute(request_assetEntitlement);

            dynamic assetEntitlement_array = JsonConvert.DeserializeObject(response_assetEntitlement.Content.ToString());

            string assetEntitlement = assetEntitlement_array.ToString();
            assetEntitlement = assetEntitlement.Remove(0, 1);
            assetEntitlement = assetEntitlement.Remove(assetEntitlement.Length - 1, 1);

            JObject json = JObject.Parse(assetEntitlement);
            return json;
        }

        protected JObject GetDellAssetHeader(string serviceTag)
        {
            var client_assetHeader = new RestClient(GET_URI_header);

            var request_assetHeader = new RestRequest(Method.GET);
            request_assetHeader.AddHeader("authorization", "Bearer " + GetDellAccessToken());
            request_assetHeader.AddParameter("servicetags", serviceTag);

            IRestResponse response_assetHeader = client_assetHeader.Execute(request_assetHeader);

            dynamic assetHeader_array = JsonConvert.DeserializeObject(response_assetHeader.Content.ToString());

            string assetHeader = assetHeader_array.ToString();
            assetHeader = assetHeader.Remove(0, 1);
            assetHeader = assetHeader.Remove(assetHeader.Length - 1, 1);

            JObject json = JObject.Parse(assetHeader);
            return json;
        }

        private void btnComputerDellLookup_Click(object sender, EventArgs e)
        {
            string serviceTag = tbComputerServiceTag.Text.ToUpper();
            if (serviceTag == "" || serviceTag == null)
            {
                MessageBox.Show("Please enter a valid Dell Service Tag");
            }

            JObject json = GetDellAssetEntitlement(serviceTag);

            tbComputerShippedDate.Text = json["shipDate"].ToString();
            tbComputerModel.Text = json["systemDescription"].ToString();

            computerPurchasedDate = json["shipDate"].ToString();
            computerModel = json["productLineDescription"].ToString();
          

            string string_numOfWarranties = json["entitlements"].Count<JToken>().ToString();
            int numOfWarranties = int.Parse(string_numOfWarranties);

            //Array to display warranty boxes
            Label[] lbls = new Label[3];
            lbls[0] = lblComputerWarranty1;
            lbls[1] = lblComputerWarranty2;
            lbls[2] = lblComputerWarranty3;
            TextBox[] tbs = new TextBox[3];
            tbs[0] = tbComputerWarranty1;
            tbs[1] = tbComputerWarranty2;
            tbs[2] = tbComputerWarranty3;

            //Display warranty boxes and info
            for (int i = 0; i < numOfWarranties && i<3; i++)
            {
                lbls[i].Visible = true;
                tbs[i].Visible = true;
                lbls[i].Text = json["entitlements"][i]["serviceLevelDescription"].ToString();
                tbs[i].Text = json["entitlements"][i]["endDate"].ToString();
            }

        }

        private void lblComputerWarranty_Click(object sender, EventArgs e)
        {

        }

        private void btnComputerPrint_Click(object sender, EventArgs e)
        {
            btnComputerDellLookup_Click(sender,e);
            //Check to make sure all the info is entered
            if (tbComputerPCName.Text == null || tbComputerPCName.Text == "")
            {
                MessageBox.Show("Please enter the PC Name");
            }
            if(tbComputerUsersName.Text == null||tbComputerUsersName.Text == "")
            {
                MessageBox.Show("Please enter the User's Full Name");
            }
            if (tbComputerServiceTag.Text == null || tbComputerServiceTag.Text == "")
            {
                MessageBox.Show("Please enter the Service Tag");
            }
            if (tbComputerWarranty1.Text == null || tbComputerWarranty1.Text == "")
            {
                MessageBox.Show("Please enter the Warranty Information");
            }

            //build protocol
            string content = null;
            switch (cbComputerLocation.SelectedIndex)
            {
                case 0:
                    content = "Top,";
                    break;
                case 1:
                    content = "OP,";
                    break;
                default:
                    //Throw error
                    MessageBox.Show("Please select a location");
                    break;
            }
            content += "c";
            //parse warranty info (keeps date, gets rid of time)
            string a = tbComputerWarranty1.Text;
            string[] b = a.Split(' ');
            //Finish building protocol
            content += "," + tbComputerPCName.Text + "," + tbComputerUsersName.Text + ","
                + tbComputerTodaysDate.Text + "," + b[0] + "," + tbComputerServiceTag.Text + ",;";

            //connect
            TcpClient client = new TcpClient(); ;
            NetworkStream ds;
            //Server connection
            try
            {
                var result = client.BeginConnect(serverIP, port, null, null);

                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

                if (!success)
                {
                    throw new SocketException();
                }


                //client = new TcpClient(serverIP, port);

                Byte[] data = Encoding.ASCII.GetBytes(content);

                ds = client.GetStream();

                ds.Write(data, 0, data.Length);

                ds.Close();
                client.Close();
                //client.Close();
            }
            catch (ArgumentNullException e2)
            {
                MessageBox.Show(e2.Message);
                client.Close();
            }
            catch (SocketException e1)
            {
                MessageBox.Show("Could not connect to network socket\nVerify that the IP is correct");
                client.Close();
            }
        }

        //ITAM Asset creation and moving

        private void btnComputerCreateAsset_Click(object sender, EventArgs e)
        {
            btnComputerDellLookup.PerformClick();
            CompAssetXMLCreation();
            AssetXMLMove();
        }

        public void CompAssetXMLCreation()
        {


            string TempPath = Path.GetTempPath();
            DateTime TrackItAuditDate = DateTime.Now.AddHours(6); //modify time to GMT for Trackit Server
            DateTime AssetAuditDate = DateTime.Now;
            //Create Workstation Tag within XML as well as all attributes
            //all attributes are required even if blank, for "Merge Audit Data" to work correctly
            XElement Workstation = new XElement("Workstation");
            XAttribute number = new XAttribute("Number", tbComputerServiceTag.Text);
            XAttribute compType = new XAttribute("CompType", "0");
            XAttribute type = new XAttribute("Type", "Baseline");
            XAttribute auditDate = new XAttribute("AuditDate", TrackItAuditDate.ToString("yyyyMMddHHmmss"));
            XAttribute auditVersion = new XAttribute("AuditVersion", "11.4.1.558");
            XAttribute auditXMLVerion = new XAttribute("AuditXMLVersion", "6.0.0");
            XAttribute workstationGuid = new XAttribute("WorkstationGuid", Guid.NewGuid().ToString().ToUpper());
            XAttribute MergePromptData = new XAttribute("MergePromptData", "True");
            XAttribute Workstation_Name = new XAttribute("Name", "IT-Asset-Cage");

            //placeholder  attributes
            XAttribute EmployeeID = new XAttribute("EmployeeID", "");
            XAttribute Comment = new XAttribute("Comment", "");
            XAttribute EMail = new XAttribute("EMail", "");
            XAttribute PhoneExt = new XAttribute("PhoneExt", "");
            XAttribute Phone = new XAttribute("Phone", "");
            XAttribute FaxExt = new XAttribute("FaxExt", "");
            XAttribute Fax = new XAttribute("Fax", "");
            XAttribute Position = new XAttribute("Position", "");
            XAttribute Workstation_Network = new XAttribute("Network", "");
            XAttribute Location = new XAttribute("Location", "");
            XAttribute DeptNum = new XAttribute("DeptNum", "");
            XAttribute Dept = new XAttribute("Dept", "");
            XAttribute UserDef_1 = new XAttribute("UserDef_1", "");
            XAttribute UserDef_2 = new XAttribute("UserDef_2", "");

            XAttribute purchaseDate = new XAttribute("UserDef1", computerPurchasedDate);
            XAttribute warrantyExpiration = new XAttribute("UserDef2", tbComputerWarranty1);
            XAttribute lastScanned = new XAttribute("UserDef3", AssetAuditDate.ToString("MM/dd/yyyy HH:mm:ss"));

            XAttribute UserDef4 = new XAttribute("UserDef4", "");
            XAttribute UserDef5 = new XAttribute("UserDef5", "");
            XAttribute UserDef6 = new XAttribute("UserDef6", "");

            //create the "Registered Applications for ServiceTag - Model and HardDrive

            XElement RegisteredApplications = new XElement("RegisteredApplications");
            XElement RegisteredAppHardDrive = new XElement("RegisteredApp");
            XAttribute hardDriveDescription = new XAttribute("SerialNumber", computerHDSerial);
            XAttribute HardDriveSerialNumber = new XAttribute("Name", computerHDSerial);

            XElement Hardware = new XElement("Hardware");
            XElement Computer = new XElement("Computer");
            XElement name = new XElement("Name", tbComputerServiceTag.Text);
            XElement manufacturer = new XElement("Manufacturer", "Dell");
            XElement model = new XElement("Model", computerModel);
            XElement serviceTag = new XElement("ServiceTag", tbComputerServiceTag.Text);
            XElement PhysicalDrive = new XElement("PhysicalDrive");
            XElement Network = new XElement("Network");
            XElement User = new XElement("User", @"MHCO\IT-Asset-Cage");
            XElement Domain = new XElement("Domain", "MHCO");

            //build XML file by adding attributes to Tags
            Workstation.Add(number);
            Workstation.Add(compType);
            Workstation.Add(type);
            Workstation.Add(auditDate);
            Workstation.Add(auditVersion);
            Workstation.Add(auditXMLVerion);
            Workstation.Add(MergePromptData);
            Workstation.Add(Workstation_Name);
            Workstation.Add(EmployeeID);
            Workstation.Add(Comment);
            Workstation.Add(EMail);
            Workstation.Add(PhoneExt);
            Workstation.Add(Phone);
            Workstation.Add(FaxExt);
            Workstation.Add(Fax);
            Workstation.Add(Position);
            Workstation.Add(Workstation_Network);
            Workstation.Add(Location);
            Workstation.Add(DeptNum);
            Workstation.Add(Dept);
            Workstation.Add(workstationGuid);
            Workstation.Add(purchaseDate);
            Workstation.Add(warrantyExpiration);
            Workstation.Add(lastScanned);
            Workstation.Add(UserDef4);
            Workstation.Add(UserDef5);
            Workstation.Add(UserDef6);
            Workstation.Add(UserDef_1);
            Workstation.Add(UserDef_2);

            RegisteredAppHardDrive.Add(HardDriveSerialNumber);
            RegisteredAppHardDrive.Add(hardDriveDescription);

            //build XML file with Tags being added within other Tags
            Workstation.Add(Hardware);
            Hardware.Add(Computer);
            Hardware.Add(Network);
            Computer.Add(name);
            Computer.Add(manufacturer);
            Computer.Add(model);
            Computer.Add(serviceTag);
            Network.Add(User);
            Network.Add(Domain);
            Workstation.Add(RegisteredApplications);
            RegisteredApplications.Add(RegisteredAppHardDrive);

            //SAVE XML FILE TO TEMP LOCATION
            Workstation.Save(TempPath + @"\865309.xml");



        }

        public void AssetXMLMove()
        {

            //Moves temporary file found at %userappdata%/temp/865309.xml to 
            //Track-it server to be processed in "Merge Audit Data" Function

            string TempPath = Path.GetTempPath();
            int Filename = 900000;
            try
            {
                while (File.Exists(@"\\mhco-trackit\TrackIt\Data\" + Filename + ".xml"))
                {
                    Filename++;
                }
                File.Move(TempPath + @"\865309.xml", @"\\mhco-trackit\TrackIt\Data\" + Filename + ".xml");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());

            }


            MessageBox.Show(string.Format("XML Created for:\r\n{0}", tbComputerServiceTag.Text));
        }
    }
}
