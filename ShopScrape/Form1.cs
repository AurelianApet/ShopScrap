using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Security.Cryptography.X509Certificates;
using HtmlAgilityPack;
using System.Text.RegularExpressions;


namespace ShopScrape
{
    public partial class Form1 : Form
    {
        struct DETAILIMAGES
        {
            public string detail_url;
            public string zoomed_url;
            public string style_name;
            public string style_id;
            public string sold_out;
            public string description;
        }

        struct SIZE {
            public string value;
            public string name;
        }

        struct DETAIL {
            public string name;
            public string model;
            public string description;
            public string price;
            public string addAction;
            public string removeAction;
            public bool added;
            public bool selected;
            public int selectedStyle;
            public int selectedSize;
            public List<DETAILIMAGES> styles_images;
            public List<SIZE> sizeList;
        }

        struct INFO {
            public string catagory;
            public bool newState;
            public bool soldOut;
            public string detailUrl;
            public string imgUrl;
            public DETAIL detail;
        }

        struct COUNTRY
        {
            public string country;
            public List<string> states;
        }

        struct COOKIE
        {
            public string cookie_name;
            public string cookie_value;
        }

        string googleKey = "fd8d546bb19b7f14fc93295f3f6d31d1";
        string captchaKey = "";

        public bool successFlag = false;

        private string urlStr = "http://www.supremenewyork.com/shop/all";
        private HttpClientHandler handler = new HttpClientHandler();
        private CookieContainer container = null;
        private HttpClient client = null;
        private List<INFO> productList = new List<INFO>();
        private INFO[] products = null;
        private string token = "";
        private bool checkoutStarted = false;
        private bool termChecked = false;
        List<COUNTRY> countries = new List<COUNTRY>();
        private string verifyKey = "";
        System.Net.Cookie[] cookies = null;
        private List<string> styles = new List<string>();
        private List<string> sizes = new List<string>();
        private string payment_token = "";
        private string payment_authe_token = "";
        private List<System.Net.Cookie> payment_cookie = new List<System.Net.Cookie>();
        
        IWebDriver driver;


        public Form1()
        {
            InitializeComponent();
            DateTime datetime = DateTime.Now;
            for(int i = 0; i < 5; i++)
            {
                YEAR_COMBO.Items.Add(Convert.ToString(datetime.Year + i));
            }
            YEAR_COMBO.SelectedIndex = 0;
            MONTH_COMBO.Text = Convert.ToString(datetime.Month);
        }

        private void initHttpClient(CookieContainer container)
        {
            //container = new CookieContainer();
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            Uri uri = new Uri(urlStr);
            //handler.CookieContainer = container;
            /*container.Add(new Cookie("__utma", "74692624.876747114.1507537096.1507879237.1507889398.3") { Domain = uri.Host });
            container.Add(new Cookie("__utmc", "74692624") { Domain = uri.Host });
            container.Add(new Cookie("_ga", "GA1.2.876747114.1507537096") { Domain = uri.Host });
            container.Add(new Cookie("_gat", "1") { Domain = uri.Host });
            container.Add(new Cookie("_gid", "GA1.2.1912941189.1507877101") { Domain = uri.Host });
            container.Add(new Cookie("lastid", "1507978136543") { Domain = uri.Host });
            container.Add(new Cookie("mp_c5c3c493b693d7f413d219e72ab974b2_mixpanel", "%7B%22distinct_id%22%3A%20%2215f00368ebe586-0d23e1a9a94dbd-c303767-1fa400-15f00368ebf5d9%22%2C%22Store%20Location%22%3A%20%22US%20Web%22%2C%22%24initial_referrer%22%3A%20%22%24direct%22%2C%22%24initial_referring_domain%22%3A%20%22%24direct%22%7D") { Domain = uri.Host });
            container.Add(new Cookie("mp_mixpanel__c", "0") { Domain = uri.Host });
            container.Add(new Cookie("request_method", "POST") { Domain = uri.Host });
            container.Add(new Cookie("tohru", "86dea7f5-bdc9-4952-bb1c-1606c921b273") { Domain = uri.Host });
            container.Add(new Cookie("uid", "9824f625c745d95297cb0fc1f8f53980") { Domain = uri.Host });
            container.Add(new Cookie("lastid", "1507899525511") { Domain = uri.Host });*/
            client = new HttpClient(/*handler*/);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            //client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8");
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            //client.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Host", uri.Host);
        }

        private void getDataFromSite()
        {
            Uri uri = new Uri(urlStr);
            try {
                Invoke(new MethodInvoker(() => showLog("Start getting products from supreme")));
                initHttpClient(container);

                var result = client.GetAsync(uri).Result;
                result.EnsureSuccessStatusCode();
                string strContent = result.Content.ReadAsStringAsync().Result;

                token = strContent.Substring(strContent.IndexOf("content=\"", strContent.IndexOf("name=\"csrf-token\"")) + "content=\"".Length + 1, strContent.IndexOf("\"", strContent.IndexOf("content=\"", strContent.IndexOf("name=\"csrf-token\"")) + "content=\"".Length + 1) - strContent.IndexOf("content=\"", strContent.IndexOf("name=\"csrf-token\"")) - "content=\"".Length - 1);
                string shopList_str = strContent.Substring(strContent.IndexOf("id=\"container\"") + "id=\"container\"".Length + 1, strContent.IndexOf("<footer", strContent.IndexOf("id=\"container\"")) - strContent.IndexOf("id=\"container\"") - "id=\"container\"".Length - 13);
                //string shopList_str = strContent.Substring(strContent.IndexOf("id=\"container\"") + "id=\"container\"".Length + 1, strContent.Length - strContent.IndexOf("id=\"container\"") - "id=\"container\"".Length - 1);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("<div> " + shopList_str + "</div>");
                //xmlDoc.LoadXml("<div> " + shopList_str);
                XmlNodeList nodes = xmlDoc.SelectNodes("div/article");
                productList.Clear();
                foreach (XmlNode node in nodes)
                {
                    string inner = node.InnerXml;
                    if (inner.IndexOf("sold out") > -1)
                    {
                        continue;
                    }
                    else
                    {

                    }
                    INFO product = new INFO();
                    product.detail = new DETAIL();
                    product.detail.styles_images = new List<DETAILIMAGES>();

                    product.detailUrl = inner.Substring(inner.IndexOf("href=\"") + "href=\"".Length, inner.IndexOf("\"", inner.IndexOf("href=\"") + "href=\"".Length) - inner.IndexOf("href=\"") - "href=\"".Length);
                    product.soldOut = false;
                    product.imgUrl = inner.Substring(inner.IndexOf("src=\"") + "src=\"".Length, inner.IndexOf("\"", inner.IndexOf("src=\"") + "src=\"".Length) - inner.IndexOf("src=\"") - "src=\"".Length);
                    productList.Add(product);
                }

                if (productList.Count == 0)
                {
                    ShowProducts();
                    return;
                }
                //products = new INFO[productList.Count];
                //MessageBox.Show(Convert.ToString(productList.Count));
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
                Invoke(new MethodInvoker(() => showLog("To connect \"supremenewyork.com \" is Failed.")));
                REFRESH_BTN.Enabled = true;
            }
            
            string host = uri.Host;
            List<INFO> temps = new List<INFO>();
            for (int i = 0; i < productList.Count; i++)
            {
                try
                {
                    //container = new CookieContainer();
                    //handler.CookieContainer = container;
                    client = new HttpClient(/*handler*/);
                    INFO product = new INFO();
                    product.detailUrl = productList[i].detailUrl;
                    uri = new Uri("http://" + host + productList[i].detailUrl);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "http://" + host + productList[i].detailUrl);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-XHR-Referer", "http://www.supremenewyork.com/shop/all");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Host", uri.Host);
                    var result = client.GetAsync(uri).Result;
                    result.EnsureSuccessStatusCode();
                    if (result.StatusCode != HttpStatusCode.OK)
                        continue;
                    string strContent = result.Content.ReadAsStringAsync().Result;
                    string shopList_str = strContent.Substring(strContent.IndexOf("id=\"details\"") + "id=\"details\"".Length + 1, strContent.IndexOf("<footer", strContent.IndexOf("id=\"details\"")) - strContent.IndexOf("id=\"details\"") - "id=\"details\"".Length - 19);
                    product.catagory = shopList_str.Substring(shopList_str.IndexOf("data-category=\"") + "data-category=\"".Length + 1, shopList_str.IndexOf("\"", shopList_str.IndexOf("data-category=\"") + "data-category=\"".Length + 1) - shopList_str.IndexOf("data-category=\"") - "data-category=\"".Length - 1);
                    product.detail.name = shopList_str.Substring(shopList_str.IndexOf("itemprop=\"name\"") + "itemprop=\"name\"".Length + 1, shopList_str.IndexOf("</h1>", shopList_str.IndexOf("itemprop=\"name\"") + "itemprop=\"name\"".Length + 1) - shopList_str.IndexOf("itemprop=\"name\"") - "itemprop=\"name\"".Length - 1);
                    product.detail.model = shopList_str.Substring(shopList_str.IndexOf("itemprop=\"model\"") + "itemprop=\"model\"".Length + 1, shopList_str.IndexOf("</p>", shopList_str.IndexOf("itemprop=\"model\"") + "itemprop=\"model\"".Length + 1) - shopList_str.IndexOf("itemprop=\"model\"") - "itemprop=\"model\"".Length - 1);
                    product.detail.description = shopList_str.Substring(shopList_str.IndexOf("itemprop=\"description\"") + "itemprop=\"description\"".Length + 1, shopList_str.IndexOf("</p>", shopList_str.IndexOf("itemprop=\"description\"") + "itemprop=\"description\"".Length + 1) - shopList_str.IndexOf("itemprop=\"description\"") - "itemprop=\"description\"".Length - 1);
                    product.detail.addAction = shopList_str.Substring(shopList_str.IndexOf("action=\"", shopList_str.IndexOf("id=\"cart-addf\"")) + "action=\"".Length + 1, shopList_str.IndexOf("\"", shopList_str.IndexOf("action=\"", shopList_str.IndexOf("id=\"cart-addf\"")) + "action=\"".Length + 1) - shopList_str.IndexOf("action=\"", shopList_str.IndexOf("id=\"cart-addf\"")) - "action=\"".Length - 1);
                    product.detail.removeAction = shopList_str.Substring(shopList_str.IndexOf("action=\"", shopList_str.IndexOf("id=\"cart-remove\"")) + "action=\"".Length + 1, shopList_str.IndexOf("\"", shopList_str.IndexOf("action=\"", shopList_str.IndexOf("id=\"cart-remove\"")) + "action=\"".Length + 1) - shopList_str.IndexOf("action=\"", shopList_str.IndexOf("id=\"cart-remove\"")) - "action=\"".Length - 1);
                    //MessageBox.Show("step1");
                    string styleListStr = "";
                    if (shopList_str.IndexOf("<ul class=\"styles") > 0)
                    {
                        if (shopList_str.IndexOf("</ul>", shopList_str.IndexOf("<ul class=\"styles")) > 0)
                            styleListStr = shopList_str.Substring(shopList_str.IndexOf("<li>", shopList_str.IndexOf("<ul class=\"styles")), shopList_str.IndexOf("</ul>", shopList_str.IndexOf("<ul class=\"styles")) - shopList_str.IndexOf("<li>", shopList_str.IndexOf("<ul class=\"styles")));
                    }
                    product.detail.price = shopList_str.Substring(shopList_str.IndexOf("itemprop=\"price\"") + "itemprop=\"price\"".Length + 1, shopList_str.IndexOf("</span>", shopList_str.IndexOf("itemprop=\"price\"")) - shopList_str.IndexOf("itemprop=\"price\"") - "itemprop=\"price\"".Length - 1);
                    string sizeListStr = "";
                    if (shopList_str.IndexOf("<option") > 0)
                    {
                        if (shopList_str.IndexOf("</select>", shopList_str.IndexOf("<option")) > 0)
                        {
                            sizeListStr = shopList_str.Substring(shopList_str.IndexOf("<option", shopList_str.IndexOf("<select")), shopList_str.IndexOf("</select>", shopList_str.IndexOf("<option", shopList_str.IndexOf("<select"))) - shopList_str.IndexOf("<option", shopList_str.IndexOf("<select")));
                        }
                    }
                    //MessageBox.Show("step2");
                    XmlDocument styleDoc = new XmlDocument();
                    styleDoc.LoadXml("<ul>" + styleListStr + "</ul>");
                    XmlNodeList styleNodes = styleDoc.SelectNodes("ul/li");
                    List<DETAILIMAGES> styleList = new List<DETAILIMAGES>();
                    foreach (XmlNode node in styleNodes)
                    {
                        string innerStr = node.InnerXml;
                        DETAILIMAGES style = new DETAILIMAGES();
                        style.style_name = innerStr.Substring(innerStr.IndexOf("data-style-name=\"") + "data-style-name=\"".Length, innerStr.IndexOf("\"", innerStr.IndexOf("data-style-name=\"") + "data-style-name=\"".Length) - innerStr.IndexOf("data-style-name=\"") - "data-style-name=\"".Length);
                        style.style_id = innerStr.Substring(innerStr.IndexOf("data-style-id=\"") + "data-style-id=\"".Length, innerStr.IndexOf("\"", innerStr.IndexOf("data-style-id=\"") + "data-style-id=\"".Length) - innerStr.IndexOf("data-style-id=\"") - "data-style-id=\"".Length);
                        style.sold_out = innerStr.Substring(innerStr.IndexOf("data-sold-out=\"") + "data-sold-out=\"".Length, innerStr.IndexOf("\"", innerStr.IndexOf("data-sold-out=\"") + "data-sold-out=\"".Length) - innerStr.IndexOf("data-sold-out=\"") - "data-sold-out=\"".Length);
                        style.description = innerStr.Substring(innerStr.IndexOf("data-description=\"") + "data-description=\"".Length, innerStr.IndexOf("\"", innerStr.IndexOf("data-description=\"") + "data-description=\"".Length) - innerStr.IndexOf("data-description=\"") - "data-description=\"".Length);
                        style.detail_url = innerStr.Substring(innerStr.IndexOf("src=\"") + "src=\"".Length, innerStr.IndexOf("\"", innerStr.IndexOf("src=\"") + "src=\"".Length) - innerStr.IndexOf("src=\"") - "src=\"".Length);
                        styleList.Add(style);
                    }
                    //MessageBox.Show("step3");
                    List<SIZE> sizeList = new List<SIZE>();
                    if (sizeListStr.Length > 0)
                    {
                        XmlDocument sizeDoc = new XmlDocument();
                        sizeDoc.LoadXml("<size>" + sizeListStr + "</size>");
                        XmlNodeList sizeNodes = sizeDoc.SelectNodes("size/option");
                        foreach (XmlNode node in sizeNodes)
                        {
                            SIZE oneSize = new SIZE();
                            oneSize.name = node.InnerText;
                            string value = node.OuterXml;
                            oneSize.value = value.Substring(value.IndexOf("value=\"") + "value=\"".Length, value.IndexOf("\"", value.IndexOf("value=\"") + "value=\"".Length) - value.IndexOf("value=\"") - "value=\"".Length);
                            sizeList.Add(oneSize);
                        }
                    }
                    //MessageBox.Show("step4");
                    product.detail.styles_images = styleList;
                    product.detail.sizeList = sizeList;
                    product.detail.added = false;
                    product.detail.selectedStyle = -1;
                    product.detail.selectedSize = -1;
                    temps.Add(product);
                    //MessageBox.Show(Convert.ToString(i));
                }
                catch (Exception e)
                {
                    //MessageBox.Show(e.ToString());
                    //Invoke(new MethodInvoker(() => showLog("To connect \"supremenewyork.com \" is Failed.")));
                    REFRESH_BTN.Enabled = true;
                }
            }
            if(temps.Count > 0)
            {
                products = new INFO[temps.Count];
                for(int i = 0; i < temps.Count; i++)
                {
                    products[i] = temps[i];
                }
            }
            if(products == null)
                Invoke(new MethodInvoker(() => showLog("We got 0 products from supreme")));
            else
                Invoke(new MethodInvoker(() => showLog("We got " + products.Length + " products from supreme")));
            ShowProducts();
        }

        private void REFRESH_BTN_Click(object sender, EventArgs e)
        {
            REFRESH_BTN.Enabled = false;
            getDataFromSite();
        }

        private void ShowProducts()
        {
            if (products == null)
                return;
            CATEGORY_LIST.Items.Clear();
            for(int i = 0; i < products.Length; i++)
            {
                if (products[i].catagory == null)
                    continue;
                CATEGORY_LIST.Items.Add(products[i].catagory);
            }
            
            NAME_TXT.Text = "";
            MODEL_TXT.Text = "";
            PRICE_TXT.Text = "";
            STYLE_COMBO.Items.Clear();
            SIZE_COMBO.Items.Clear();
            DESCRIPTION.Text = "";
            REFRESH_BTN.Enabled = true;
        }

        private void CATEGORY_LIST_MouseClick(object sender, MouseEventArgs e)
        {
            
            int selectedIndex = CATEGORY_LIST.SelectedIndex;
            if (selectedIndex == -1)
                return;
            NAME_TXT.Text = products[selectedIndex].detail.name;
            MODEL_TXT.Text = products[selectedIndex].detail.model;
            PRICE_TXT.Text = products[selectedIndex].detail.price;
            STYLE_COMBO.Items.Clear();
            for(int i = 0; i < products[selectedIndex].detail.styles_images.Count; i ++)
            {
                STYLE_COMBO.Items.Add(products[selectedIndex].detail.styles_images[i].style_name);
            }
            if(STYLE_COMBO.Items.Count > 0)
                STYLE_COMBO.SelectedIndex = 0;
            SIZE_COMBO.Items.Clear();
            for (int i = 0; i < products[selectedIndex].detail.sizeList.Count; i++)
            {
                SIZE_COMBO.Items.Add(products[selectedIndex].detail.sizeList[i].name);
            }
            if(SIZE_COMBO.Items.Count > 0)
                SIZE_COMBO.SelectedIndex = 0;
            DESCRIPTION.Text = products[selectedIndex].detail.description;
            if(products[selectedIndex].detail.added == false)
            {
                ADDED.Visible = false;
            }
            else
            {
                ADDED.Visible = true;
            }
        }

        private void ADDBUSTKET_BTN_Click(object sender, EventArgs e)
        {
            int selectedIndex = CATEGORY_LIST.SelectedIndex;
            if(selectedIndex < 0)
            {
                MessageBox.Show("Please select item to add.");
                return;
            }
            if(products[selectedIndex].detail.added == true)
            {
                MessageBox.Show("Cannot add selected Item in basket ");
                return;
            }

            BASKET_LIST.Items.Add(products[selectedIndex].detail.name);
            for(int i = 0; i < products.Length; i++)
            {
                if (products[i].catagory == products[selectedIndex].catagory)
                    products[i].detail.added = true;
            }
            products[selectedIndex].detail.selected = true;
            ADDED.Visible = true;
            products[selectedIndex].detail.selectedStyle = STYLE_COMBO.SelectedIndex;
            products[selectedIndex].detail.selectedSize = SIZE_COMBO.SelectedIndex;
        }

        private void BASKET_LIST_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < products.Length; i++)
            {
                if(products[i].detail.name == BASKET_LIST.SelectedItem)
                {
                    BUY_STYLE.Text = products[i].detail.styles_images[products[i].detail.selectedStyle].style_name;
                    BUY_SIZE.Text = products[i].detail.sizeList[products[i].detail.selectedSize].name;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CATEGORY_LIST.SelectedIndex < 0)
                return;
            int selectedIndex = CATEGORY_LIST.SelectedIndex;
            if(selectedIndex < 0)
            {
                MessageBox.Show("Please select item to remove.");
                return;
            }

            for (int i = 0; i < products.Length; i++)
            {
                if (products[i].detail.name == BASKET_LIST.SelectedItem)
                {
                    products[i].detail.added = false;
                    products[i].detail.selected = false;
                    products[i].detail.selectedStyle = -1;
                    products[i].detail.selectedSize = -1;
                    for(int j = 0; j < products.Length; j++)
                    {
                        if(products[i].catagory == products[j].catagory)
                        {
                            products[j].detail.added = false;
                            products[j].detail.selected = false;
                        }
                    }
                }
            }

            BASKET_LIST.Items.RemoveAt(BASKET_LIST.SelectedIndex);
        }

        private async void CHECKOUT_Click(object sender, EventArgs e)
        {
            CheckOut();
        }

        private void CANCEL_BTN_Click(object sender, EventArgs e)
        {
            checkoutStarted = false;
            ADDBUSTKET_BTN.Enabled = true;
            button1.Enabled = true;
            CHECKOUT.Enabled = true;
            PAYMENT_BTN.Enabled = false;
            try
            {
                
            }catch(Exception ex)
            {

            }

        }

        private void PAYMENT_BTN_Click(object sender, EventArgs e)
        {
            //if (CheckOut() == false)
                //return;
            CHECKOUT.Enabled = false;
            PAYMENT_BTN.Enabled = false;
            if (FULLNAME_TXT.Text == "")
            {
                MessageBox.Show("Please Enter Name.");
                return;
            }
            if(EMAIL_TXT.Text == "")
            {
                MessageBox.Show("Please Enter Email.");
                return;
            }
            if(TEL_TXT.Text.Length < 10)
            {
                MessageBox.Show("must be a 10 digital us Telephone.");
                return;
            }
            if (ADDRESS_TXT.Text == "")
            {
                MessageBox.Show("Please Enter Address.");
                return;
            }
            if(CITY_TXT.Text == "")
            {
                MessageBox.Show("Please Enter City.");
                return;
            }
            if(POSTCODE.Text == "")
            {
                MessageBox.Show("Please Enter postcode.");
                return;
            }
            if (NUMBER_TXT.Text == "")
            {
                MessageBox.Show("Please Enter Number.");
                return;
            }
            if(CVV_TXT.Text == "")
            {
                MessageBox.Show("Please Enter CVV.");
                return;
            }

            bool chromeShowed = false;
            try {
                //PhantomJSDriverService service = PhantomJSDriverService.CreateDefaultService();
                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;
                
                //PhantomJSOptions option = new PhantomJSOptions();
                ChromeOptions option = new ChromeOptions();
                option.AddArgument("--window-position=-32000,-32000");
                
                //driver = new PhantomJSDriver(service);
                driver = new ChromeDriver(service, option);
                //driver.Manage().Cookies.DeleteAllCookies();
                driver.Navigate().GoToUrl("http://supremenewyork.com");
                chromeShowed = true;
                Uri uri1 = new Uri("https://supremenewyork.com/checkout");
                for (int i = 0; i < payment_cookie.Count; i++)
                {
                    OpenQA.Selenium.Cookie cookie = new OpenQA.Selenium.Cookie(payment_cookie[i].Name, payment_cookie[i].Value, uri1.Host, "/", DateTime.Now.AddDays(1));
                    driver.Manage().Cookies.AddCookie(cookie);
                }
                driver.Navigate().GoToUrl(uri1);
                driver.FindElement(By.Id("order_billing_name")).Clear();
                driver.FindElement(By.Id("order_billing_name")).SendKeys(FULLNAME_TXT.Text);
                System.Threading.Thread.Sleep(100);
                driver.FindElement(By.Id("order_email")).Clear();
                driver.FindElement(By.Id("order_email")).SendKeys(EMAIL_TXT.Text);
                System.Threading.Thread.Sleep(100);
                driver.FindElement(By.Id("order_tel")).Clear();
                driver.FindElement(By.Id("order_tel")).SendKeys(TEL_TXT.Text);
                System.Threading.Thread.Sleep(100);
                driver.FindElement(By.Id("bo")).Clear();
                driver.FindElement(By.Id("bo")).SendKeys(ADDRESS_TXT.Text);
                System.Threading.Thread.Sleep(100);
                driver.FindElement(By.Id("oba3")).Clear();
                driver.FindElement(By.Id("oba3")).SendKeys(ADDRESS2_TXT.Text);
                System.Threading.Thread.Sleep(100);
                driver.FindElement(By.Id("order_billing_zip")).Clear();
                driver.FindElement(By.Id("order_billing_zip")).SendKeys(POSTCODE.Text);
                System.Threading.Thread.Sleep(100);
                
                driver.FindElement(By.Id("nnaerb")).Clear();
                for (int j = 0; j < NUMBER_TXT.Text.Length; j++)
                    driver.FindElement(By.Id("nnaerb")).SendKeys(NUMBER_TXT.Text.Substring(j, 1));
                System.Threading.Thread.Sleep(100);
                driver.FindElement(By.Id("credit_card_month")).SendKeys(MONTH_COMBO.Text);
                System.Threading.Thread.Sleep(100);
                driver.FindElement(By.Id("credit_card_year")).SendKeys(YEAR_COMBO.Text);
                System.Threading.Thread.Sleep(100);
                driver.FindElement(By.Id("orcer")).Clear();
                driver.FindElement(By.Id("orcer")).SendKeys(CVV_TXT.Text);
                System.Threading.Thread.Sleep(100);
                var elements = driver.FindElements(By.ClassName("icheckbox_minimal"));
                //if (termChecked == false)
                {
                    elements[elements.Count - 1].Click();
                    termChecked = true;
                }
                var element1 = driver.FindElement(By.ClassName("g-recaptcha"));
                var token = driver.FindElement(By.Name("csrf-token"));
                string token_str = token.GetAttribute("content");
                //captchaKey = driver.FindElement(By.ClassName("g-recaptcha")).GetAttribute("data-sitekey");
                CookieContainer container1 = new CookieContainer();
                HttpClientHandler handler1 = new HttpClientHandler();
                handler1.CookieContainer = container1;

                var sd = driver.Manage().Cookies.AllCookies;
                for (int i = 0; i < payment_cookie.Count; i++)
                {
                    container1.Add(new System.Net.Cookie(payment_cookie[i].Name, payment_cookie[i].Value) { Domain = payment_cookie[i].Domain });
                }
                /*for (int i = 0; i < sd.Count; i++)
                {
                    container1.Add(new System.Net.Cookie(sd[i].Name, sd[i].Value) { Domain = sd[i].Domain });
                }*/
                Invoke(new MethodInvoker(() => showLog("Getting the Recaptcha key.")));
                chromeShowed = false;
                driver.Close();
                driver.Quit();
                
                verifyKey = trySolvingCaptcha();
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                HttpClient client1 = new HttpClient(handler1);
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Content-Length", "1500");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.supremenewyork.com/checkout");
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("X-CSRF-Token", token_str);
                client1.DefaultRequestHeaders.TryAddWithoutValidation("X-CSRF-Token", payment_token);
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://www.supremenewyork.com");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Host", "www.supremenewyork.com");
                var postData = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("utf8", "✓"),
                            //new KeyValuePair<string, string>("authenticity_token", token_str),
                            new KeyValuePair<string, string>("authenticity_token", payment_token),
                            new KeyValuePair<string, string>("order[billing_name]", FULLNAME_TXT.Text),
                            new KeyValuePair<string, string>("order[email]", EMAIL_TXT.Text),
                            new KeyValuePair<string, string>("order[tel]", TEL_TXT.Text),
                            new KeyValuePair<string, string>("order[billing_address]", ADDRESS_TXT.Text),
                            new KeyValuePair<string, string>("order[billing_address_2]", ADDRESS2_TXT.Text),
                            new KeyValuePair<string, string>("order[billing_zip]", POSTCODE.Text),
                            new KeyValuePair<string, string>("order[billing_city]", CITY_TXT.Text),
                            new KeyValuePair<string, string>("order[billing_state]", STATE_COMBO.Text),
                            new KeyValuePair<string, string>("order[billing_country]", COUNTRY_COMBO.Text),
                            new KeyValuePair<string, string>("same_as_billing_address", "1"),
                            new KeyValuePair<string, string>("store_credit_id", ""),
                            new KeyValuePair<string, string>("credit_card[cnb]", NUMBER_TXT.Text),
                            new KeyValuePair<string, string>("credit_card[month]", MONTH_COMBO.Text),
                            new KeyValuePair<string, string>("credit_card[year]", YEAR_COMBO.Text),
                            new KeyValuePair<string, string>("credit_card[vval]", CVV_TXT.Text),
                            new KeyValuePair<string, string>("order[terms]", "0"),
                            new KeyValuePair<string, string>("order[terms]", "1"),
                            new KeyValuePair<string, string>("g-recaptcha-response", verifyKey),
                        });
                Uri uri = new Uri("https://supremenewyork.com/checkout.json");
                var result = client1.PostAsync(uri, postData).Result;
                result.EnsureSuccessStatusCode();
                string responseMessageCheckoutPostString = result.Content.ReadAsStringAsync().Result;
                string slug = "";
                if(responseMessageCheckoutPostString.Contains("slug"))
                {
                    string[] items = responseMessageCheckoutPostString.Split(',');
                    for(int i = 0; i < items.Length; i++)
                    {
                        if (items[i].Contains("slug"))
                        {
                            string []subitems = items[i].Split(':');
                            slug = subitems[1];
                            slug = slug.Replace("\"", "");
                            slug = slug.Replace("}", "");
                        }
                    }
                }
                else
                {
                    MessageBox.Show(responseMessageCheckoutPostString, "Fail");
                }
                if(slug != "")
                {
                    uri = new Uri("https://supremenewyork.com/checkout/" + slug + "/status.json");
                    result = client1.GetAsync(uri).Result;
                    result.EnsureSuccessStatusCode();
                    responseMessageCheckoutPostString = result.Content.ReadAsStringAsync().Result;
                    if (responseMessageCheckoutPostString.Contains("failed"))
                    {
                        MessageBox.Show(responseMessageCheckoutPostString);
                        Invoke(new MethodInvoker(() => showLog("Payment Fail.")));
                    }
                    else
                    {
                        Invoke(new MethodInvoker(() => showLog("Payment Success.")));
                    }
                }

                ADDBUSTKET_BTN.Enabled = true;
                button1.Enabled = true;
                CHECKOUT.Enabled = true;
                //PAYMENT_BTN.Enabled = false;




                /*

                Invoke(new MethodInvoker(() => showLog("Getting the Recaptcha key.")));
                verifyKey = trySolvingCaptcha();
                Invoke(new MethodInvoker(() => showLog("Recaptcha key = " + verifyKey)));
                CookieContainer container1 = new CookieContainer();
                HttpClientHandler handler1 = new HttpClientHandler();
                handler1.CookieContainer = container1;
                for (int i = 0; i < payment_cookie.Count; i++)
                {
                    container1.Add(payment_cookie[i]);
                }
                HttpClient client1 = new HttpClient(handler1);*/
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8");
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("Content-Length", "1500");
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.supremenewyork.com/checkout");
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("X-CSRF-Token", payment_token);
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://www.supremenewyork.com");
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("Host", "www.supremenewyork.com");

                //Invoke(new MethodInvoker(() => showLog("step1")));
                //string uriStr = "https://www.supremenewyork.com/checkout.js?utf8=%E2%9C%93&authenticity_token=" + payment_token + "&order%5Bbilling_name%5D=" + FULLNAME_TXT.Text.Replace(" ", "+") + "&order%5Bemail%5D=" + EMAIL_TXT.Text.Replace("@", "%40") + "&order%5Btel%5D=" + TEL_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address%5D=" + ADDRESS_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address_2%5D=" + ADDRESS2_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_zip%5D=" + POSTCODE.Text.Replace(" ", "+") + "&order%5Bbilling_city%5D=" + CITY_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_state%5D=" + STATE_COMBO.Text.Replace(" ", "+") + "&order%5Bbilling_country%5D=" + COUNTRY_COMBO.Text.Replace(" ", "+") + "&same_as_billing_address=1&store_credit_id=&credit_card%5Bcnb%5D=&credit_card%5Bmonth%5D=" + MONTH_COMBO.Text + "&credit_card%5Byear%5D=" + YEAR_COMBO.Text + "&credit_card%5Bvval%5D=&order%5Bterms%5D=0&g-recaptcha-response=&cnt=1";
                //string uriStr = "https://www.supremenewyork.com/checkout.js?utf8=%E2%9C%93&order%5Bbilling_name%5D=" + FULLNAME_TXT.Text.Replace(" ", "+") + "&order%5Bemail%5D=" + EMAIL_TXT.Text.Replace("@", "%40") + "&order%5Btel%5D=" + TEL_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address%5D=" + ADDRESS_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address_2%5D=" + ADDRESS2_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_zip%5D=" + POSTCODE.Text.Replace(" ", "+") + "&order%5Bbilling_city%5D=" + CITY_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_state%5D=" + STATE_COMBO.Text.Replace(" ", "+") + "&order%5Bbilling_country%5D=" + COUNTRY_COMBO.Text.Replace(" ", "+") + "&same_as_billing_address=1&store_credit_id=&credit_card%5Bcnb%5D=&credit_card%5Bmonth%5D=" + MONTH_COMBO.Text + "&credit_card%5Byear%5D=" + YEAR_COMBO.Text + "&credit_card%5Bvval%5D=&order%5Bterms%5D=0&g-recaptcha-response=&cnt=1";

                //Uri uri = new Uri(uriStr);
                //var result = client1.GetAsync(uri).Result;
                //result.EnsureSuccessStatusCode();
                /*
                CookieContainer container2 = new CookieContainer();
                HttpClientHandler handler2 = new HttpClientHandler();
                handler2.CookieContainer = container2;
                for (int i = 0; i < payment_cookie.Count; i++)
                {
                    container2.Add(payment_cookie[i]);
                }
                HttpClient client2 = new HttpClient(handler2);*/
                //client2.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
                //client1.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
                //client2.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8");
                //client2.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
                //client2.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                //client2.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.supremenewyork.com/checkout");
                //client2.DefaultRequestHeaders.TryAddWithoutValidation("X-CSRF-Token", payment_token);
                //client2.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
                //client2.DefaultRequestHeaders.TryAddWithoutValidation("Host", "www.supremenewyork.com");
                //uriStr = "https://www.supremenewyork.com/checkout.js?utf8=%E2%9C%93&authenticity_token=" + payment_token + "&order%5Bbilling_name%5D=" + FULLNAME_TXT.Text.Replace(" ", "+") + "&order%5Bemail%5D=" + EMAIL_TXT.Text.Replace("@", "%40") + "&order%5Btel%5D=" + TEL_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address%5D=" + ADDRESS_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address_2%5D=" + ADDRESS2_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_zip%5D=" + POSTCODE.Text.Replace(" ", "+") + "&order%5Bbilling_city%5D=" + CITY_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_state%5D=" + STATE_COMBO.Text.Replace(" ", "+") + "&order%5Bbilling_country%5D=" + COUNTRY_COMBO.Text.Replace(" ", "+") + "&same_as_billing_address=1&store_credit_id=&credit_card%5Bcnb%5D=&credit_card%5Bmonth%5D=" + MONTH_COMBO.Text + "&credit_card%5Byear%5D=" + YEAR_COMBO.Text + "&credit_card%5Bvval%5D=&order%5Bterms%5D=0&g-recaptcha-response=&cnt=2";
                //uriStr = "https://www.supremenewyork.com/checkout.js?utf8=%E2%9C%93&order%5Bbilling_name%5D=" + FULLNAME_TXT.Text.Replace(" ", "+") + "&order%5Bemail%5D=" + EMAIL_TXT.Text.Replace("@", "%40") + "&order%5Btel%5D=" + TEL_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address%5D=" + ADDRESS_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address_2%5D=" + ADDRESS2_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_zip%5D=" + POSTCODE.Text.Replace(" ", "+") + "&order%5Bbilling_city%5D=" + CITY_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_state%5D=" + STATE_COMBO.Text.Replace(" ", "+") + "&order%5Bbilling_country%5D=" + COUNTRY_COMBO.Text.Replace(" ", "+") + "&same_as_billing_address=1&store_credit_id=&credit_card%5Bcnb%5D=&credit_card%5Bmonth%5D=" + MONTH_COMBO.Text + "&credit_card%5Byear%5D=" + YEAR_COMBO.Text + "&credit_card%5Bvval%5D=&order%5Bterms%5D=0&g-recaptcha-response=&cnt=2";
                //uri = new Uri(uriStr);
                //result = client2.GetAsync(uri).Result;
                //result.EnsureSuccessStatusCode();
                //uriStr = "https://www.supremenewyork.com/checkout.js?utf8=%E2%9C%93&authenticity_token=" + payment_token + "&order%5Bbilling_name%5D=" + FULLNAME_TXT.Text.Replace(" ", "+") + "&order%5Bemail%5D=" + EMAIL_TXT.Text.Replace("@", "%40") + "&order%5Btel%5D=" + TEL_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address%5D=" + ADDRESS_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address_2%5D=" + ADDRESS2_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_zip%5D=" + POSTCODE.Text.Replace(" ", "+") + "&order%5Bbilling_city%5D=" + CITY_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_state%5D=" + STATE_COMBO.Text.Replace(" ", "+") + "&order%5Bbilling_country%5D=" + COUNTRY_COMBO.Text.Replace(" ", "+") + "&same_as_billing_address=1&store_credit_id=&credit_card%5Bcnb%5D=&credit_card%5Bmonth%5D=" + MONTH_COMBO.Text + "&credit_card%5Byear%5D=" + YEAR_COMBO.Text + "&credit_card%5Bvval%5D=&order%5Bterms%5D=0&g-recaptcha-response=&cnt=3";
                //uriStr = "https://www.supremenewyork.com/checkout.js?utf8=%E2%9C%93&order%5Bbilling_name%5D=" + FULLNAME_TXT.Text.Replace(" ", "+") + "&order%5Bemail%5D=" + EMAIL_TXT.Text.Replace("@", "%40") + "&order%5Btel%5D=" + TEL_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address%5D=" + ADDRESS_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address_2%5D=" + ADDRESS2_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_zip%5D=" + POSTCODE.Text.Replace(" ", "+") + "&order%5Bbilling_city%5D=" + CITY_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_state%5D=" + STATE_COMBO.Text.Replace(" ", "+") + "&order%5Bbilling_country%5D=" + COUNTRY_COMBO.Text.Replace(" ", "+") + "&same_as_billing_address=1&store_credit_id=&credit_card%5Bcnb%5D=&credit_card%5Bmonth%5D=" + MONTH_COMBO.Text + "&credit_card%5Byear%5D=" + YEAR_COMBO.Text + "&credit_card%5Bvval%5D=&order%5Bterms%5D=0&g-recaptcha-response=&cnt=3";
                //uri = new Uri(uriStr);
                //result = client2.GetAsync(uri).Result;
                //result.EnsureSuccessStatusCode();
                //uriStr = "https://www.supremenewyork.com/checkout.js?utf8=%E2%9C%93&authenticity_token=" + payment_token + "&order%5Bbilling_name%5D=" + FULLNAME_TXT.Text.Replace(" ", "+") + "&order%5Bemail%5D=" + EMAIL_TXT.Text.Replace("@", "%40") + "&order%5Btel%5D=" + TEL_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address%5D=" + ADDRESS_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address_2%5D=" + ADDRESS2_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_zip%5D=" + POSTCODE.Text.Replace(" ", "+") + "&order%5Bbilling_city%5D=" + CITY_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_state%5D=" + STATE_COMBO.Text.Replace(" ", "+") + "&order%5Bbilling_country%5D=" + COUNTRY_COMBO.Text.Replace(" ", "+") + "&same_as_billing_address=1&store_credit_id=&credit_card%5Bcnb%5D=&credit_card%5Bmonth%5D=" + MONTH_COMBO.Text + "&credit_card%5Byear%5D=" + YEAR_COMBO.Text + "&credit_card%5Bvval%5D=&order%5Bterms%5D=0&g-recaptcha-response=&cnt=2";
                //uriStr = "https://www.supremenewyork.com/checkout.js?utf8=%E2%9C%93&order%5Bbilling_name%5D=" + FULLNAME_TXT.Text.Replace(" ", "+") + "&order%5Bemail%5D=" + EMAIL_TXT.Text.Replace("@", "%40") + "&order%5Btel%5D=" + TEL_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address%5D=" + ADDRESS_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_address_2%5D=" + ADDRESS2_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_zip%5D=" + POSTCODE.Text.Replace(" ", "+") + "&order%5Bbilling_city%5D=" + CITY_TXT.Text.Replace(" ", "+") + "&order%5Bbilling_state%5D=" + STATE_COMBO.Text.Replace(" ", "+") + "&order%5Bbilling_country%5D=" + COUNTRY_COMBO.Text.Replace(" ", "+") + "&same_as_billing_address=1&store_credit_id=&credit_card%5Bcnb%5D=&credit_card%5Bmonth%5D=" + MONTH_COMBO.Text + "&credit_card%5Byear%5D=" + YEAR_COMBO.Text + "&credit_card%5Bvval%5D=&order%5Bterms%5D=0&g-recaptcha-response=&cnt=4";
                //uri = new Uri(uriStr);
                //result = client2.GetAsync(uri).Result;
                //result.EnsureSuccessStatusCode();
                //uriStr = "https://www.supremenewyork.com/store_credits/verify?email=" + EMAIL_TXT.Text.Replace("@", "%40");
                //uri = new Uri(uriStr);
                //result = client2.GetAsync(uri).Result;
                //result.EnsureSuccessStatusCode();
                //client2.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "http://www.supremenewyork.com");
                //client2.DefaultRequestHeaders.TryAddWithoutValidation("Content-Length", "1500");
                //client2.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                //var postData = new FormUrlEncodedContent(new[]
                //        {
                //            new KeyValuePair<string, string>("utf8", "✓"),
                //            new KeyValuePair<string, string>("authenticity_token", payment_token),
                //            new KeyValuePair<string, string>("order[billing_name]", FULLNAME_TXT.Text),
                //            new KeyValuePair<string, string>("order[email]", EMAIL_TXT.Text),
                //            new KeyValuePair<string, string>("order[tel]", TEL_TXT.Text),
                //            new KeyValuePair<string, string>("order[billing_address]", ADDRESS_TXT.Text),
                //            new KeyValuePair<string, string>("order[billing_address_2]", ADDRESS2_TXT.Text),
                //            new KeyValuePair<string, string>("order[billing_zip]", POSTCODE.Text),
                //            new KeyValuePair<string, string>("order[billing_city]", CITY_TXT.Text),
                //            new KeyValuePair<string, string>("order[billing_state]", STATE_COMBO.Text),
                //            new KeyValuePair<string, string>("order[billing_country]", COUNTRY_COMBO.Text),
                //            new KeyValuePair<string, string>("same_as_billing_address", "1"),
                //            new KeyValuePair<string, string>("store_credit_id", "visa"),
                //            new KeyValuePair<string, string>("credit_card[cnb]", NUMBER_TXT.Text),
                //            new KeyValuePair<string, string>("credit_card[month]", MONTH_COMBO.Text),
                //            new KeyValuePair<string, string>("credit_card[year]", YEAR_COMBO.Text),
                //            new KeyValuePair<string, string>("credit_card[vval]", CVV_TXT.Text),
                //            new KeyValuePair<string, string>("order[terms]", "0"),
                //            new KeyValuePair<string, string>("order[terms]", "1"),
                //            new KeyValuePair<string, string>("g-recaptcha-response", verifyKey),
                //        });
                //Invoke(new MethodInvoker(() => showLog("step1")));
                //uri = new Uri("https://www.supremenewyork.com/checkout.json");
                //result = client2.PostAsync(uri, postData).Result;
                //result.EnsureSuccessStatusCode();
                //string responseMessageCheckoutPostString = result.Content.ReadAsStringAsync().Result;
                //string slug = responseMessageCheckoutPostString.Substring(responseMessageCheckoutPostString.IndexOf("slug\":\"") + "slug\":\"".Length, responseMessageCheckoutPostString.IndexOf("\"", responseMessageCheckoutPostString.IndexOf("slug\":\"") + "slug\":\"".Length) - responseMessageCheckoutPostString.IndexOf("slug\":\"") - "slug\":\"".Length);
                //uri = new Uri("https://www.supremenewyork.com/checkout/" + slug + "/status.json");
                //result = client2.GetAsync(uri).Result;
                //result.EnsureSuccessStatusCode();
                //responseMessageCheckoutPostString = result.Content.ReadAsStringAsync().Result;
                //MessageBox.Show(responseMessageCheckoutPostString);
                //Invoke(new MethodInvoker(() => showLog("step2")));
                //string orderInfo = string.Empty;
                /*if (!isCheckout(responseMessageCheckoutPostString, ref orderInfo))
                {
                    string errorString = responseMessageCheckoutPostString.Substring(responseMessageCheckoutPostString.IndexOf("id=\"confirmation\"") + "id=\"confirmation\"".Length + 4, responseMessageCheckoutPostString.IndexOf("/p", responseMessageCheckoutPostString.IndexOf("id=\"confirmation\"") + "id=\"confirmation\"".Length + 4) - responseMessageCheckoutPostString.IndexOf("id=\"confirmation\"") - "id=\"confirmation\"".Length - 5);
                    MessageBox.Show(errorString, "Failed");
                    Invoke(new MethodInvoker(() => showLog(errorString)));
                }
                else
                {
                    MessageBox.Show(orderInfo, "Success");
                    Invoke(new MethodInvoker(() => showLog(orderInfo)));
                }*/
                //Invoke(new MethodInvoker(() => showLog("step3")));
                //CHECKOUT.Enabled = true;
                /*

                                driver.FindElement(By.Id("order_billing_name")).Clear();
                                driver.FindElement(By.Id("order_billing_name")).SendKeys(FULLNAME_TXT.Text);
                                System.Threading.Thread.Sleep(100);
                                driver.FindElement(By.Id("order_email")).Clear();
                                driver.FindElement(By.Id("order_email")).SendKeys(EMAIL_TXT.Text);
                                System.Threading.Thread.Sleep(100);
                                driver.FindElement(By.Id("order_tel")).Clear();
                                driver.FindElement(By.Id("order_tel")).SendKeys(TEL_TXT.Text);
                                System.Threading.Thread.Sleep(100);
                                driver.FindElement(By.Id("bo")).Clear();
                                driver.FindElement(By.Id("bo")).SendKeys(ADDRESS_TXT.Text);
                                System.Threading.Thread.Sleep(100);
                                driver.FindElement(By.Id("oba3")).Clear();
                                driver.FindElement(By.Id("oba3")).SendKeys(ADDRESS2_TXT.Text);
                                System.Threading.Thread.Sleep(100);
                                driver.FindElement(By.Id("order_billing_address_3")).Clear();
                                driver.FindElement(By.Id("order_billing_address_3")).SendKeys(ADDRESS3_TXT.Text);
                                System.Threading.Thread.Sleep(100);
                                driver.FindElement(By.Id("order_billing_city")).Clear();
                                driver.FindElement(By.Id("order_billing_city")).SendKeys(CITY_TXT.Text);
                                System.Threading.Thread.Sleep(100);
                                driver.FindElement(By.Id("order_billing_zip")).Clear();
                                driver.FindElement(By.Id("order_billing_zip")).SendKeys(POSTCODE.Text);
                                System.Threading.Thread.Sleep(100);
                                driver.FindElement(By.Id("order_billing_country")).SendKeys(COUNTRY_COMBO.Text);
                                System.Threading.Thread.Sleep(100);
                                driver.FindElement(By.Id("cnb")).Clear();
                                for(int j = 0; j < NUMBER_TXT.Text.Length; j++)
                                    driver.FindElement(By.Id("cnb")).SendKeys(NUMBER_TXT.Text.Substring(j, 1));
                                System.Threading.Thread.Sleep(100);
                                driver.FindElement(By.Id("credit_card_month")).SendKeys(MONTH_COMBO.Text);
                                System.Threading.Thread.Sleep(100);
                                driver.FindElement(By.Id("credit_card_year")).SendKeys(YEAR_COMBO.Text);
                                System.Threading.Thread.Sleep(100);
                                driver.FindElement(By.Id("vval")).Clear();
                                driver.FindElement(By.Id("vval")).SendKeys(CVV_TXT.Text);
                                System.Threading.Thread.Sleep(100);
                                var elements = driver.FindElements(By.ClassName("icheckbox_minimal"));
                                if(termChecked == false)
                                {
                                    elements[elements.Count - 1].Click();
                                    termChecked = true;
                                }

                                captchaKey = driver.FindElement(By.ClassName("g-recaptcha")).GetAttribute("data-sitekey");

                                IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                                var element1 = driver.FindElement(By.ClassName("g-recaptcha"));
                                string inner = element1.GetAttribute("innerHTML");
                                executor.ExecuteScript(string.Format("arguments[0].innerHTML = '<noscript>' + {0} + '</noscript>'; ", inner), element1);
                                System.Threading.Thread.Sleep(10000);
                                verifyKey = trySolvingCaptcha();
                                var element = driver.FindElement(By.Id("g-recaptcha-response"));

                                executor.ExecuteScript("arguments[0].style.display = 'block';", element);
                                executor.ExecuteScript("arguments[0].style.resize = 'block';", element);
                                System.Threading.Thread.Sleep(500);
                                executor.ExecuteScript(string.Format("arguments[0].innerHTML = '{0}';", verifyKey), element);
                                System.Threading.Thread.Sleep(2000);
                                driver.FindElement(By.Name("commit")).Click();
                                System.Threading.Thread.Sleep(2000);

                                ADDBUSTKET_BTN.Enabled = true;
                                button1.Enabled = true;
                                CHECKOUT.Enabled = true;
                                PAYMENT_BTN.Enabled = false;
                                driver.Close();
                                driver.Quit();*/
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Invoke(new MethodInvoker(() => showLog("To connect \"supremenewyork.com \" is Failed.")));
                checkoutStarted = false;
                ADDBUSTKET_BTN.Enabled = true;
                button1.Enabled = true;
                CHECKOUT.Enabled = true;
                //PAYMENT_BTN.Enabled = false;
            }
            if (chromeShowed == true)
            {
                driver.Close();
                driver.Quit();
            }
        }

        private void COUNTRY_COMBO_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private void COUNTRY_COMBO_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (COUNTRY_COMBO.SelectedIndex == -1)
                return;
            if (countries.Count < 1)
                return;
            int selected_Index = COUNTRY_COMBO.SelectedIndex;
            STATE_COMBO.Items.Clear();
            for(int i = 0; i < countries[selected_Index].states.Count; i++)
            {
                STATE_COMBO.Items.Add(countries[selected_Index].states[i]);
            }
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
        
        private string trySolvingCaptcha()
        {
            string verifyCode = string.Empty;
            HttpClient httpClient = new HttpClient();
            try
            {
                string sendUrl = string.Format("http://2captcha.com/in.php?key={0}&method=userrecaptcha&googlekey={1}&pageurl=www.supremenewyork.com/checkout", googleKey, captchaKey);
                HttpResponseMessage responseMessageMain = httpClient.GetAsync(sendUrl).Result;
                responseMessageMain.EnsureSuccessStatusCode();
                string sendUrlString = responseMessageMain.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(sendUrlString))
                    return verifyCode;


                if (!sendUrlString.Contains("OK|"))
                    return verifyCode;


                string captchaId = sendUrlString.Replace("OK|", string.Empty);
                if (string.IsNullOrEmpty(captchaId))
                    return verifyCode;


                string verifyUrl = string.Format("http://2captcha.com/res.php?key={0}&action=get&id={1}", googleKey, captchaId);


                int requestCount = 0;
                while (requestCount < 20)
                {
                    System.Threading.Thread.Sleep(15000);
                    requestCount++;


                    HttpResponseMessage responseMessageVerify = httpClient.GetAsync(verifyUrl).Result;
                    responseMessageVerify.EnsureSuccessStatusCode();


                    string verifyUrlString = responseMessageVerify.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(verifyUrlString))
                        continue;


                    if (!verifyUrlString.Contains("OK|") && verifyUrlString.Contains("CAPCHA_NOT_READY"))
                        continue;


                    verifyCode = verifyUrlString.Replace("OK|", string.Empty);
                    break;
                }


                return verifyCode;
            }
            catch (Exception e)
            {
                return verifyCode;
            }
        }

        private bool isCheckout(string response, ref string orderInfo)
        {
            try
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(response);

                IEnumerable<HtmlNode> nodeContent = doc.DocumentNode.Descendants("div").Where(node => node.Attributes["id"] != null && node.Attributes["id"].Value == "content");
                if (nodeContent == null || nodeContent.LongCount() < 1)
                    return false;

                IEnumerable<HtmlNode> nodeConfirm = nodeContent.ToArray()[0].Descendants("div").Where(node => node.Attributes["id"] != null && node.Attributes["id"].Value == "confirmation");
                if (nodeConfirm == null || nodeConfirm.LongCount() < 1)
                    return false;

                IEnumerable<HtmlNode> nodePs = nodeConfirm.ToArray()[0].Descendants("p");
                if (nodePs == null || nodePs.LongCount() < 2)
                    return false;

                string confirmContent = nodePs.ToArray()[0].InnerText.Trim();
                if (string.IsNullOrEmpty(confirmContent))
                    return false;

                GroupCollection groups = Regex.Match(confirmContent, "Order:\\s*#(?<order>\\d*)").Groups;
                if (groups == null || groups["order"] == null)
                    return false;

                string orderId = groups["order"].Value;
                if (string.IsNullOrEmpty(orderId))
                    return false;

                orderInfo = string.Format("Order: {0}\r\n{1}", orderId, nodePs.ToArray()[1].InnerText.Trim());

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void showLog(string log)
        {
            LOG_TEXT.Text += log + "\r\n";
            LOG_TEXT.SelectionStart = LOG_TEXT.TextLength;
            LOG_TEXT.ScrollToCaret();
        }

        private bool CheckOut()
        {
            if (BASKET_LIST.Items.Count == 0)
            {
                MessageBox.Show("Please add item to buy.");
                return false;
            }
            try
            {
                CHECKOUT.Enabled = false;
                Invoke(new MethodInvoker(() => showLog("Start action to add the products to basket.")));
                Uri uri = new Uri(urlStr);
                string host = uri.Host;
                CookieContainer container1 = new CookieContainer();
                HttpClientHandler handler1 = new HttpClientHandler();
                handler1.CookieContainer = container1;
                container1.Add(new System.Net.Cookie("mp_c5c3c493b693d7f413d219e72ab974b2_mixpanel", "%7B%22distinct_id%22%3A%20%2215f2979ee801f5-0bdc5b097aaa4d-c303767-1fa400-15f2979ee814d1%22%2C%22%24initial_referrer%22%3A%20%22%24direct%22%2C%22%24initial_referring_domain%22%3A%20%22%24direct%22%7D") { Domain = uri.Host });
                container1.Add(new System.Net.Cookie("mp_mixpanel__c", "0") { Domain = uri.Host });
                HttpClient client1 = new HttpClient(handler1);
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*;q=0.5, text/javascript, application/javascript, application/ecmascript, application/x-ecmascript");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Content-Length", "58");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "http://" + host);
                client1.DefaultRequestHeaders.TryAddWithoutValidation("X-CSRF-Token", token);
                client1.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
                client1.DefaultRequestHeaders.TryAddWithoutValidation("Host", uri.Host);
                string XHR_Referer = "";
                styles.Clear();
                sizes.Clear();
                for (int i = 0; i < products.Length; i++)
                {
                    if (products[i].detail.selected == true)
                    {

                        client1.DefaultRequestHeaders.Remove("Referer");
                        XHR_Referer = "http://" + host + products[i].detailUrl;
                        client1.DefaultRequestHeaders.TryAddWithoutValidation("Referer", XHR_Referer);
                        uri = new Uri("http://" + host + "/" + products[i].detail.addAction);
                        byte[] bytes = new byte[] { 0xE2, 0x9C, 0x93 };
                        string utf8 = System.Text.Encoding.UTF8.GetString(bytes);
                        var postData = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("utf8", utf8),
                            new KeyValuePair<string, string>("style", Convert.ToString(products[i].detail.styles_images[products[i].detail.selectedStyle].style_id)),
                            new KeyValuePair<string,string>("size", Convert.ToString(products[i].detail.sizeList[products[i].detail.selectedSize].value)),
                            new KeyValuePair<string,string>("commit", "add to basket"),
                        });
                        styles.Add(Convert.ToString(products[i].detail.styles_images[products[i].detail.selectedStyle].style_id));
                        sizes.Add(Convert.ToString(products[i].detail.sizeList[products[i].detail.selectedSize].value));
                        var result = client1.PostAsync(uri, postData).Result;
                        Invoke(new MethodInvoker(() => showLog("\"" + products[i].detail.name + "\" is added to the basket.")));
                    }
                }
                Invoke(new MethodInvoker(() => showLog("All products are added to the basked successly.")));
                var responseCookies = container1.GetCookies(uri).Cast<System.Net.Cookie>();
                client1.Dispose();
                uri = new Uri("http://" + host + "/shop/cart");
                CookieContainer container2 = new CookieContainer();
                HttpClientHandler handler2 = new HttpClientHandler();
                handler2.CookieContainer = container2;
                int count = 0;
                foreach (var cookie in responseCookies)
                {
                    container2.Add(new System.Net.Cookie(cookie.Name, cookie.Value) { Domain = uri.Host });
                    count++;
                }
                cookies = new System.Net.Cookie[count];
                int index = 0;
                foreach (var cookie in responseCookies)
                {
                    cookies[index] = cookie;
                    index++;
                }
                HttpClient client2 = new HttpClient(handler2);
                client2.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml");
                client2.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8");
                client2.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
                client2.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "http://" + uri.Host + "/shop/cart");
                //client2.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
                client2.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
                client2.DefaultRequestHeaders.TryAddWithoutValidation("Host", uri.Host);

                var result1 = client2.GetAsync(uri).Result;
                System.Threading.Thread.Sleep(100);
                result1.EnsureSuccessStatusCode();
                string strContent = result1.Content.ReadAsStringAsync().Result;
                string totalprice = strContent.Substring(strContent.IndexOf("subtotal-span") + "subtotal-span".Length + 3, strContent.IndexOf("span", strContent.IndexOf("subtotal-span") + "subtotal-span".Length + 3) - strContent.IndexOf("subtotal-span") - "subtotal-span".Length - 5);

                var responseCookies2 = container2.GetCookies(uri).Cast<System.Net.Cookie>();
                CookieContainer container3 = new CookieContainer();
                HttpClientHandler handler3 = new HttpClientHandler();
                uri = new Uri("https://" + host + "/checkout");
                handler3.CookieContainer = container3;
                bool pure_cart = false;
                string cart = "";
                string pure_cart_Str = "";
                foreach (var cookie in responseCookies2)
                {
                    if (cookie.Name == "pure_cart")
                    {
                        pure_cart = true;
                        pure_cart_Str = cookie.Value;
                    }
                    if (cookie.Name == "cart")
                    {
                        cart = cookie.Value;
                    }
                    for (int i = 0; i < cookies.Length; i++)
                    {
                        if (cookies[i].Name == cookie.Name)
                        {
                            cookies[i].Value = cookie.Value;
                        }
                    }
                }
                for (int i = 0; i < cookies.Length; i++)
                {
                    container3.Add(new System.Net.Cookie(cookies[i].Name, cookies[i].Value) { Domain = uri.Host });
                }

                if (pure_cart == false)
                {
                    pure_cart_Str = "%7B%22";
                    for (int i = 0; i < styles.Count; i++)
                    {
                        pure_cart_Str += styles[i];
                        pure_cart_Str += "%22%3A1%2C%22";
                    }
                    pure_cart_Str += "cookie%22%3A%22";
                    pure_cart_Str += cart;
                    pure_cart_Str += "%22%2C%22total%22%3A%22%E2%82%AC";
                    pure_cart_Str += totalprice;
                    pure_cart_Str += "%22%7D";
                    pure_cart_Str = pure_cart_Str.Replace("+", "%20");
                }
                else
                {

                }

                if (pure_cart == false)
                    container3.Add(new System.Net.Cookie("pure_cart", pure_cart_Str) { Domain = uri.Host });
                //container3.Add(new System.Net.Cookie("tohru", "f3b626ec-f81d-432c-becc-e37b015088b2") { Domain = uri.Host });
                container3.Add(new System.Net.Cookie("__utmz", "74692624.1508576657.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none)") { Domain = uri.Host });

                //container3.Add(new System.Net.Cookie("_gat", "1") { Domain = uri.Host });
                //container3.Add(new System.Net.Cookie("__utmb", "74692624.7.10.1508525390") { Domain = uri.Host });
                //container3.Add(new System.Net.Cookie("__utma", "74692624.1971999489.1508525390.1508525390.1508525390.1") { Domain = uri.Host });
                //container3.Add(new System.Net.Cookie("__utmc", "74692624") { Domain = uri.Host });
                //container3.Add(new System.Net.Cookie("__utmt", "1") { Domain = uri.Host });
                //container3.Add(new System.Net.Cookie("_ga", "GA1.2.1971999489.1508525390") { Domain = uri.Host });
                //container3.Add(new System.Net.Cookie("_gid", "GA1.2.1811082608.1508525390") { Domain = uri.Host });
                //container3.Add(new System.Net.Cookie("__utmb", "74692624.7.10.1508525390") { Domain = uri.Host });
                //handler3.ClientCertificateOptions = ClientCertificateOption.Automatic;
                HttpClient client3 = new HttpClient(handler3);
                client3.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
                client3.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                //client3.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
                client3.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8");
                client3.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
                client3.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "http://" + uri.Host + "/shop/cart");
                client3.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
                client3.DefaultRequestHeaders.TryAddWithoutValidation("Host", uri.Host);

                var result3 = client3.GetAsync(uri).Result;
                System.Threading.Thread.Sleep(100);
                result3.EnsureSuccessStatusCode();
                payment_cookie.Clear();
                var responseCookies3 = container3.GetCookies(uri).Cast<System.Net.Cookie>();
                string supreme_sess = "";
                foreach (var cookie in responseCookies3)
                {
                    if (cookie.Name == "_supreme_sess")
                    {
                        supreme_sess = cookie.Value;
                    }
                }
                payment_cookie.Add(new System.Net.Cookie("_supreme_sess", supreme_sess) { Domain = uri.Host });
                payment_cookie.Add(new System.Net.Cookie("cart", cart) { Domain = uri.Host });
                payment_cookie.Add(new System.Net.Cookie("mp_c5c3c493b693d7f413d219e72ab974b2_mixpanel", "%7B%22distinct_id%22%3A%20%2215f3a02f5be10c-0cf436420d639b-c303767-1fa400-15f3a02f5bf884%22%2C%22%24initial_referrer%22%3A%20%22%24direct%22%2C%22%24initial_referring_domain%22%3A%20%22%24direct%22%7D") { Domain = uri.Host });
                payment_cookie.Add(new System.Net.Cookie("mp_mixpanel__c", "0") { Domain = uri.Host });
                payment_cookie.Add(new System.Net.Cookie("pure_cart", pure_cart_Str) { Domain = uri.Host });

                strContent = result3.Content.ReadAsStringAsync().Result;
                string sub_total = strContent.Substring(strContent.IndexOf("id=\"subtotal\"") + "id=\"subtotal\"".Length + 1, strContent.IndexOf("span", strContent.IndexOf("id=\"subtotal\"")) - strContent.IndexOf("id=\"subtotal\"") - "id=\"subtotal\"".Length - 3);
                CART_TOTAL.Text = sub_total;
                string shipping = strContent.Substring(strContent.IndexOf("id=\"shipping\"") + "id=\"shipping\"".Length + 1, strContent.IndexOf("span", strContent.IndexOf("id=\"shipping\"")) - strContent.IndexOf("id=\"shipping\"") - "id=\"shipping\"".Length - 3);
                SHIP_HANDLE.Text = shipping;
                string total = strContent.Substring(strContent.IndexOf("id=\"total\"") + "id=\"total\"".Length + 1, strContent.IndexOf("strong", strContent.IndexOf("id=\"total\"")) - strContent.IndexOf("id=\"total\"") - "id=\"total\"".Length - 3);
                ORDER_TOTAL.Text = total;

                payment_token = strContent.Substring(strContent.IndexOf("content=\"", strContent.IndexOf("name=\"csrf-token\"")) + "content=\"".Length, strContent.IndexOf("\"", strContent.IndexOf("content=\"", strContent.IndexOf("name=\"csrf-token\"")) + "content=\"".Length) - strContent.IndexOf("content=\"", strContent.IndexOf("name=\"csrf-token\"")) - "content=\"".Length);
                payment_authe_token = strContent.Substring(strContent.IndexOf("value=\"", strContent.IndexOf("name=\"authenticity_token\"")) + "value=\"".Length, strContent.IndexOf("\"", strContent.IndexOf("value=\"", strContent.IndexOf("name=\"authenticity_token\"")) + "value=\"".Length) - strContent.IndexOf("value=\"", strContent.IndexOf("name=\"authenticity_token\"")) - "value=\"".Length);
                captchaKey = strContent.Substring(strContent.IndexOf("data-sitekey=\"") + "data-sitekey=\"".Length, strContent.IndexOf("\"", strContent.IndexOf("data-sitekey=\"") + "data-sitekey=\"".Length) - strContent.IndexOf("data-sitekey=\"") - "data-sitekey=\"".Length);

                /*  For Germany.
                string country_Lists = strContent.Substring(strContent.IndexOf("id=\"order_billing_country\"") + "id=\"order_billing_country\"".Length + 1, strContent.IndexOf("select", strContent.IndexOf("id=\"order_billing_country\"")) - strContent.IndexOf("id=\"order_billing_country\"") - "id=\"order_billing_country\"".Length - 3);
                country_Lists = country_Lists.Replace("\r", "");
                string[] countrys = country_Lists.Split('\n');
                COUNTRY_COMBO.Items.Clear();
                for(int i = 0; i < countrys.Length; i++)
                {
                    string country = countrys[i].Substring(countrys[i].IndexOf("value=\"") + "value=\"".Length, 2);
                    COUNTRY_COMBO.Items.Add(country);
                }
                COUNTRY_COMBO.SelectedIndex = 0;
                string type_Lists = strContent.Substring(strContent.IndexOf("id=\"credit_card_type\"") + "id=\"credit_card_type\"".Length + 1, strContent.IndexOf("select", strContent.IndexOf("id=\"credit_card_type\"")) - strContent.IndexOf("id=\"credit_card_type\"") - "id=\"credit_card_type\"".Length - 3);
                type_Lists = type_Lists.Replace("\r", "");
                string[] types = type_Lists.Split('\n');
                TYPE_COMBO.Items.Clear();
                for (int i = 0; i < types.Length; i++)
                {
                    string type = types[i].Substring(types[i].IndexOf(">", 3) + 1, types[i].IndexOf("<", types[i].IndexOf(">", 3) + 1) - types[i].IndexOf(">", 3) - 1);
                    TYPE_COMBO.Items.Add(type);
                }*/
                string country_Lists = strContent.Substring(strContent.IndexOf("id=\"order_billing_country\"") + "id=\"order_billing_country\"".Length + 1, strContent.IndexOf("/select", strContent.IndexOf("id=\"order_billing_country\"")) - strContent.IndexOf("id=\"order_billing_country\"") - "id=\"order_billing_country\"".Length - 2);
                country_Lists = country_Lists.Replace("\r", "");
                string[] countrys = country_Lists.Split('\n');
                COUNTRY_COMBO.Items.Clear();
                for (int i = 0; i < countrys.Length; i++)
                {
                    string country = countrys[i].Substring(countrys[i].IndexOf("value=\"") + "value=\"".Length, countrys[i].IndexOf("\"", countrys[i].IndexOf("value=\"") + "value=\"".Length) - countrys[i].IndexOf("value=\"") - "value=\"".Length);
                    COUNTRY_COMBO.Items.Add(country);
                }
                COUNTRY_COMBO.SelectedIndex = 0;
                countries.Clear();
                for (int i = 0; i < COUNTRY_COMBO.Items.Count; i++)
                {
                    COUNTRY temp = new COUNTRY();
                    temp.country = COUNTRY_COMBO.GetItemText(i);
                    temp.states = new List<string>();
                    string stateID = "id=\"states-" + COUNTRY_COMBO.Items[i] + "\"";
                    string state_list = strContent.Substring(strContent.IndexOf("option", strContent.IndexOf(stateID)) - 1, strContent.IndexOf("/script", strContent.IndexOf(stateID)) - strContent.IndexOf("option", strContent.IndexOf(stateID)));
                    state_list = state_list.Replace("\r", "");
                    string[] state = state_list.Split('\n');
                    for (int j = 0; j < state.Length; j++)
                    {
                        string stateValue = state[j].Substring(state[j].IndexOf("value=\"") + "value=\"".Length, state[j].IndexOf("\"", state[j].IndexOf("value=\"") + "value=\"".Length) - state[j].IndexOf("value=\"") - "value=\"".Length);
                        temp.states.Add(stateValue);
                    }
                    countries.Add(temp);
                    if (i == 0)
                    {
                        STATE_COMBO.Items.Clear();
                        for (int j = 0; j < temp.states.Count; j++)
                        {
                            STATE_COMBO.Items.Add(temp.states[j]);
                        }
                    }
                }
                COUNTRY_COMBO.SelectedIndex = 0;
                //string type_Lists = strContent.Substring(strContent.IndexOf("id=\"credit_card_type\"") + "id=\"credit_card_type\"".Length + 1, strContent.IndexOf("select", strContent.IndexOf("id=\"credit_card_type\"")) - strContent.IndexOf("id=\"credit_card_type\"") - "id=\"credit_card_type\"".Length - 3);
                //type_Lists = type_Lists.Replace("\r", "");
                //string[] types = type_Lists.Split('\n');
                //TYPE_COMBO.Items.Clear();
                /*for (int i = 0; i < types.Length; i++)
                {
                    string type = types[i].Substring(types[i].IndexOf(">", 3) + 1, types[i].IndexOf("<", types[i].IndexOf(">", 3) + 1) - types[i].IndexOf(">", 3) - 1);
                    TYPE_COMBO.Items.Add(type);
                }*/
                Invoke(new MethodInvoker(() => showLog("To calculate the price about added products is finished.")));
                Invoke(new MethodInvoker(() => showLog("Please pay.")));
                //TYPE_COMBO.SelectedIndex = 0;
                CHECKOUT.Enabled = true;
                PAYMENT_BTN.Enabled = true;
                /*
                                checkoutStarted = true;
                                ADDBUSTKET_BTN.Enabled = false;
                                button1.Enabled = false;
                                CHECKOUT.Enabled = false;
                                termChecked = false;
                                //PhantomJSDriverService service = PhantomJSDriverService.CreateDefaultService();
                                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                                service.HideCommandPromptWindow = true;
                                //driver = new PhantomJSDriver(service);
                                driver = new ChromeDriver(service);
                                PhantomJSOptions option = new PhantomJSOptions();
                                for (int i = 0; i < products.Length; i++)
                                {
                                    if (products[i].detail.selected == true)
                                    {
                                        uri = new Uri("http://" + host + products[i].detailUrl);
                                        driver.Navigate().GoToUrl(uri);
                                        driver.FindElement(By.TagName("select")).SendKeys(products[i].detail.sizeList[products[i].detail.selectedSize].name);
                                        System.Threading.Thread.Sleep(2000);
                                        var elements = driver.FindElements(By.TagName("input"));
                                        foreach(var element in elements)
                                        {
                                            if (element.Size.Width > 90 || element.Size.Height > 20)
                                            {
                                                element.Click();
                                                System.Threading.Thread.Sleep(2000);
                                                break;
                                            }
                                        }
                                    }
                                }
                                driver.Navigate().GoToUrl("https://" + host + "/checkout");
                                System.Threading.Thread.Sleep(2000);
                                string country_list = driver.FindElement(By.Id("order_billing_country")).Text;
                                country_list = country_list.Replace("\r", "");
                                string[] countrys = country_list.Split('\n');
                                if (countrys.Length > 0)
                                {
                                    COUNTRY_COMBO.Items.Clear();
                                }
                                for (int i = 0; i < countrys.Length; i++)
                                {
                                    COUNTRY_COMBO.Items.Add(countrys[i]);
                                    if(i == 0)
                                    {
                                        COUNTRY_COMBO.SelectedIndex = 0;
                                    }
                                }
                                string type_list = driver.FindElement(By.Id("credit_card_type")).Text;
                                type_list = type_list.Replace("\r", "");
                                string[] types = type_list.Split('\n');
                                if (types.Length > 0)
                                {
                                    TYPE_COMBO.Items.Clear();
                                }
                                for (int i = 0; i < types.Length; i++)
                                {
                                    TYPE_COMBO.Items.Add(types[i]);
                                    if (i == 0)
                                        TYPE_COMBO.SelectedIndex = 0;
                                }
                                string month_list = driver.FindElement(By.Id("credit_card_month")).Text;
                                month_list = month_list.Replace("\r", "");
                                string[] months = month_list.Split('\n');
                                if (months.Length > 0)
                                {
                                    MONTH_COMBO.Items.Clear();
                                }
                                for (int i = 0; i < months.Length; i++)
                                {
                                    MONTH_COMBO.Items.Add(months[i]);
                                    if (months[i] == Convert.ToString(DateTime.Now.Month))
                                        MONTH_COMBO.SelectedIndex = i;
                                }

                                string year_list = driver.FindElement(By.Id("credit_card_year")).Text;
                                year_list = year_list.Replace("\r", "");
                                string[] years = year_list.Split('\n');
                                if (years.Length > 0)
                                {
                                    YEAR_COMBO.Items.Clear();
                                }
                                for (int i = 0; i < years.Length; i++)
                                {
                                    YEAR_COMBO.Items.Add(years[i]);
                                    if (i == 0)
                                    {
                                        YEAR_COMBO.SelectedIndex = 0;
                                    }
                                }
                                string subtotal = driver.FindElement(By.Id("subtotal")).Text;
                                CART_TOTAL.Text = subtotal;
                                string shipping = driver.FindElement(By.Id("shipping")).Text;
                                SHIP_HANDLE.Text = shipping;
                                string total = driver.FindElement(By.Id("total")).Text;
                                ORDER_TOTAL.Text = total;
                                PAYMENT_BTN.Enabled = true;*/
            }
            catch (Exception ex)
            {
                Invoke(new MethodInvoker(() => showLog("To connect \"supremenewyork.com \" is Failed.")));
                checkoutStarted = false;
                ADDBUSTKET_BTN.Enabled = true;
                button1.Enabled = true;
                CHECKOUT.Enabled = true;
                PAYMENT_BTN.Enabled = false;
                return false;
                //driver.Close();
                //driver.Quit();
            }
            return true;
        }
    }
}
