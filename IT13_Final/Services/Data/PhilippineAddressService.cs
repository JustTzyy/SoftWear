using System.Collections.Generic;

namespace IT13_Final.Services.Data
{
    public interface IPhilippineAddressService
    {
        Dictionary<string, Dictionary<string, string>> GetAddressData();
        List<string> GetProvinces();
        List<string> GetCities(string province);
        string GetZipCode(string province, string city);
    }

    public sealed class PhilippineAddressService : IPhilippineAddressService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _addressData;

        public PhilippineAddressService()
        {
            _addressData = new Dictionary<string, Dictionary<string, string>>();
            InitializeData();
        }

        private void InitializeData()
        {
            // Provinces in alphabetical order
            _addressData["Bicol Region"] = new Dictionary<string, string>
            {
                { "Naga City", "4400" },
                { "Iriga City", "4431" },
                { "Baao", "4432" },
                { "Balatan", "4433" },
                { "Bato", "4434" },
                { "Bombon", "4435" },
                { "Buhi", "4436" },
                { "Bula", "4437" },
                { "Cabusao", "4438" },
                { "Calabanga", "4439" },
                { "Camaligan", "4440" },
                { "Canaman", "4441" },
                { "Caramoan", "4442" },
                { "Del Gallego", "4443" },
                { "Gainza", "4444" },
                { "Garchitorena", "4445" },
                { "Goa", "4446" },
                { "Lagonoy", "4447" },
                { "Libmanan", "4448" },
                { "Lupi", "4449" },
                { "Magarao", "4450" },
                { "Milaor", "4451" },
                { "Minalabac", "4452" },
                { "Nabua", "4453" },
                { "Ocampo", "4454" },
                { "Pamplona", "4455" },
                { "Pasacao", "4456" },
                { "Pili", "4457" },
                { "Presentacion", "4458" },
                { "Ragay", "4459" },
                { "Sagñay", "4460" },
                { "San Fernando", "4461" },
                { "San Jose", "4462" },
                { "Sipocot", "4463" },
                { "Siruma", "4464" },
                { "Tigaon", "4465" },
                { "Tinambac", "4466" }
            };

            _addressData["Biliran"] = new Dictionary<string, string>
            {
                { "Naval", "6560" },
                { "Almeria", "6561" },
                { "Biliran", "6562" },
                { "Cabucgayan", "6563" },
                { "Caibiran", "6564" },
                { "Culaba", "6565" },
                { "Kawayan", "6566" },
                { "Maripipi", "6567" }
            };

            _addressData["Bohol"] = new Dictionary<string, string>
            {
                { "Tagbilaran City", "6300" },
                { "Alburquerque", "6301" },
                { "Alicia", "6302" },
                { "Anda", "6303" },
                { "Antequera", "6304" },
                { "Baclayon", "6305" },
                { "Balilihan", "6306" },
                { "Batuan", "6307" },
                { "Bien Unido", "6308" },
                { "Bilar", "6309" },
                { "Buenavista", "6310" },
                { "Calape", "6311" },
                { "Candijay", "6312" },
                { "Carmen", "6313" },
                { "Catigbian", "6314" },
                { "Clarin", "6315" },
                { "Corella", "6316" },
                { "Cortes", "6317" },
                { "Dagohoy", "6318" },
                { "Danao", "6319" },
                { "Dauis", "6320" },
                { "Dimiao", "6321" },
                { "Duero", "6322" },
                { "Garcia Hernandez", "6323" },
                { "Guindulman", "6324" },
                { "Getafe", "6325" },
                { "Inabanga", "6326" },
                { "Jagna", "6327" },
                { "Jetafe", "6328" },
                { "Lila", "6329" },
                { "Loay", "6330" },
                { "Loboc", "6331" },
                { "Loon", "6332" },
                { "Mabini", "6333" },
                { "Maribojoc", "6334" },
                { "Panglao", "6335" },
                { "Pilar", "6336" },
                { "President Carlos P. Garcia", "6337" },
                { "Sagbayan", "6338" },
                { "San Isidro", "6339" },
                { "San Miguel", "6340" },
                { "Sevilla", "6341" },
                { "Sierra Bullones", "6342" },
                { "Sikatuna", "6343" },
                { "Talibon", "6344" },
                { "Trinidad", "6345" },
                { "Tubigon", "6346" },
                { "Ubay", "6347" },
                { "Valencia", "6348" }
            };

            _addressData["Bukidnon"] = new Dictionary<string, string>
            {
                { "Malaybalay City", "8700" },
                { "Valencia City", "8709" },
                { "Baungon", "8701" },
                { "Cabanglasan", "8702" },
                { "Damulog", "8703" },
                { "Dangcagan", "8704" },
                { "Don Carlos", "8705" },
                { "Impasugong", "8706" },
                { "Kadingilan", "8707" },
                { "Kalilangan", "8708" },
                { "Kibawe", "8710" },
                { "Kitaotao", "8711" },
                { "Lantapan", "8712" },
                { "Libona", "8713" },
                { "Malitbog", "8714" },
                { "Manolo Fortich", "8715" },
                { "Maramag", "8716" },
                { "Pangantucan", "8717" },
                { "Quezon", "8718" },
                { "San Fernando", "8719" },
                { "Sumilao", "8720" },
                { "Talakag", "8721" }
            };

            _addressData["Bulacan"] = new Dictionary<string, string>
            {
                { "Malolos City", "3000" },
                { "Meycauayan City", "3020" },
                { "San Jose del Monte City", "3023" },
                { "Angat", "3001" },
                { "Balagtas", "3016" },
                { "Baliuag", "3006" },
                { "Bocaue", "3018" },
                { "Bulakan", "3017" },
                { "Bustos", "3007" },
                { "Calumpit", "3003" },
                { "Doña Remedios Trinidad", "3009" },
                { "Guiguinto", "3015" },
                { "Hagonoy", "3002" },
                { "Marilao", "3019" },
                { "Norzagaray", "3013" },
                { "Obando", "3021" },
                { "Pandi", "3014" },
                { "Paombong", "3001" },
                { "Plaridel", "3004" },
                { "Pulilan", "3005" },
                { "San Ildefonso", "3010" },
                { "San Miguel", "3011" },
                { "San Rafael", "3008" },
                { "Santa Maria", "3022" }
            };

            _addressData["Cagayan"] = new Dictionary<string, string>
            {
                { "Tuguegarao City", "3500" },
                { "Abulug", "3501" },
                { "Alcala", "3502" },
                { "Allacapan", "3503" },
                { "Amulung", "3504" },
                { "Aparri", "3515" },
                { "Baggao", "3505" },
                { "Ballesteros", "3506" },
                { "Buguey", "3516" },
                { "Calayan", "3517" },
                { "Camalaniugan", "3507" },
                { "Claveria", "3508" },
                { "Enrile", "3509" },
                { "Gattaran", "3510" },
                { "Gonzaga", "3518" },
                { "Iguig", "3511" },
                { "Lal-lo", "3512" },
                { "Lasam", "3513" },
                { "Pamplona", "3514" },
                { "Peñablanca", "3515" },
                { "Piat", "3516" },
                { "Rizal", "3517" },
                { "Sanchez-Mira", "3518" },
                { "Santa Ana", "3519" },
                { "Santa Praxedes", "3520" },
                { "Santa Teresita", "3521" },
                { "Santo Niño", "3522" },
                { "Solana", "3523" },
                { "Tuao", "3524" }
            };

            _addressData["Camarines Norte"] = new Dictionary<string, string>
            {
                { "Daet", "4600" },
                { "Basud", "4601" },
                { "Capalonga", "4602" },
                { "Jose Panganiban", "4603" },
                { "Labo", "4604" },
                { "Mercedes", "4605" },
                { "Paracale", "4606" },
                { "San Lorenzo Ruiz", "4607" },
                { "San Vicente", "4608" },
                { "Santa Elena", "4609" },
                { "Talisay", "4610" },
                { "Vinzons", "4611" }
            };

            _addressData["Camarines Sur"] = new Dictionary<string, string>
            {
                { "Naga City", "4400" },
                { "Iriga City", "4431" },
                { "Baao", "4432" },
                { "Balatan", "4433" },
                { "Bato", "4434" },
                { "Bombon", "4435" },
                { "Buhi", "4436" },
                { "Bula", "4437" },
                { "Cabusao", "4438" },
                { "Calabanga", "4439" },
                { "Camaligan", "4440" },
                { "Canaman", "4441" },
                { "Caramoan", "4442" },
                { "Del Gallego", "4443" },
                { "Gainza", "4444" },
                { "Garchitorena", "4445" },
                { "Goa", "4446" },
                { "Lagonoy", "4447" },
                { "Libmanan", "4448" },
                { "Lupi", "4449" },
                { "Magarao", "4450" },
                { "Milaor", "4451" },
                { "Minalabac", "4452" },
                { "Nabua", "4453" },
                { "Ocampo", "4454" },
                { "Pamplona", "4455" },
                { "Pasacao", "4456" },
                { "Pili", "4457" },
                { "Presentacion", "4458" },
                { "Ragay", "4459" },
                { "Sagñay", "4460" },
                { "San Fernando", "4461" },
                { "San Jose", "4462" },
                { "Sipocot", "4463" },
                { "Siruma", "4464" },
                { "Tigaon", "4465" },
                { "Tinambac", "4466" }
            };

            _addressData["Camiguin"] = new Dictionary<string, string>
            {
                { "Mambajao", "9100" },
                { "Catarman", "9101" },
                { "Guinsiliban", "9102" },
                { "Mahinog", "9103" },
                { "Sagay", "9104" }
            };

            _addressData["Capiz"] = new Dictionary<string, string>
            {
                { "Roxas City", "5800" },
                { "Cuartero", "5801" },
                { "Dao", "5802" },
                { "Dumalag", "5803" },
                { "Dumarao", "5804" },
                { "Ivisan", "5805" },
                { "Jamindan", "5806" },
                { "Ma-ayon", "5807" },
                { "Mambusao", "5808" },
                { "Panay", "5809" },
                { "Panitan", "5810" },
                { "Pilar", "5811" },
                { "Pontevedra", "5812" },
                { "President Roxas", "5813" },
                { "Sapi-an", "5814" },
                { "Sigma", "5815" },
                { "Tapaz", "5816" }
            };

            _addressData["Catanduanes"] = new Dictionary<string, string>
            {
                { "Virac", "4800" },
                { "Bagamanoc", "4801" },
                { "Baras", "4802" },
                { "Bato", "4803" },
                { "Caramoran", "4804" },
                { "Gigmoto", "4805" },
                { "Panganiban", "4806" },
                { "Pandan", "4807" },
                { "San Andres", "4808" },
                { "San Miguel", "4809" },
                { "Viga", "4810" }
            };

            _addressData["Cavite"] = new Dictionary<string, string>
            {
                { "Cavite City", "4100" },
                { "Dasmariñas City", "4114" },
                { "Imus City", "4103" },
                { "Tagaytay City", "4120" },
                { "Trece Martires City", "4109" },
                { "Alfonso", "4123" },
                { "Amadeo", "4119" },
                { "Bacoor", "4102" },
                { "Carmona", "4116" },
                { "General Emilio Aguinaldo", "4124" },
                { "General Mariano Alvarez", "4117" },
                { "General Trias", "4107" },
                { "Indang", "4122" },
                { "Kawit", "4104" },
                { "Magallanes", "4125" },
                { "Maragondon", "4126" },
                { "Mendez", "4121" },
                { "Naic", "4110" },
                { "Noveleta", "4105" },
                { "Rosario", "4106" },
                { "Silang", "4118" },
                { "Tanza", "4108" },
                { "Ternate", "4111" }
            };

            _addressData["Cebu"] = new Dictionary<string, string>
            {
                { "Cebu City", "6000" },
                { "Lapu-Lapu City", "6015" },
                { "Mandaue City", "6014" },
                { "Talisay City", "6045" },
                { "Toledo City", "6038" },
                { "Danao City", "6004" },
                { "Bogo City", "6010" },
                { "Carcar City", "6019" },
                { "Naga City", "6037" },
                { "Alcantara", "6033" },
                { "Alcoy", "6021" },
                { "Alegria", "6030" },
                { "Aloguinsan", "6040" },
                { "Argao", "6021" },
                { "Asturias", "6042" },
                { "Badian", "6031" },
                { "Balamban", "6041" },
                { "Bantayan", "6052" },
                { "Barili", "6036" },
                { "Boljoon", "6024" },
                { "Borbon", "6008" },
                { "Carmen", "6005" },
                { "Catmon", "6006" },
                { "Compostela", "6003" },
                { "Consolacion", "6001" },
                { "Cordova", "6017" },
                { "Daanbantayan", "6013" },
                { "Dalaguete", "6022" },
                { "Dumanjug", "6035" },
                { "Ginatilan", "6032" },
                { "Liloan", "6002" },
                { "Madridejos", "6053" },
                { "Malabuyoc", "6029" },
                { "Medellin", "6012" },
                { "Minglanilla", "6046" },
                { "Moalboal", "6034" },
                { "Oslob", "6025" },
                { "Pilar", "6046" },
                { "Pinamungajan", "6039" },
                { "Poro", "6049" },
                { "Ronda", "6037" },
                { "Samboan", "6027" },
                { "San Fernando", "6018" },
                { "San Francisco", "6050" },
                { "San Remigio", "6011" },
                { "Santa Fe", "6047" },
                { "Santander", "6026" },
                { "Sibonga", "6020" },
                { "Sogod", "6007" },
                { "Tabogon", "6009" },
                { "Tabuelan", "6043" },
                { "Tuburan", "6044" },
                { "Tudela", "6051" }
            };

            _addressData["Compostela Valley"] = new Dictionary<string, string>
            {
                { "Nabunturan", "8800" },
                { "Pantukan", "8801" },
                { "Compostela", "8803" },
                { "Laak", "8804" },
                { "Mabini", "8805" },
                { "Maco", "8806" },
                { "Maragusan", "8807" },
                { "Mawab", "8809" },
                { "Monkayo", "8810" },
                { "Montevista", "8811" },
                { "New Bataan", "8812" }
            };

            _addressData["Cotabato"] = new Dictionary<string, string>
            {
                { "Kidapawan City", "9400" },
                { "Alamada", "9401" },
                { "Aleosan", "9402" },
                { "Antipas", "9403" },
                { "Arakan", "9404" },
                { "Banisilan", "9405" },
                { "Carmen", "9406" },
                { "Kabacan", "9407" },
                { "Libungan", "9408" },
                { "M'lang", "9409" },
                { "Magpet", "9410" },
                { "Makilala", "9411" },
                { "Matalam", "9412" },
                { "Midsayap", "9413" },
                { "Pigcawayan", "9414" },
                { "Pikit", "9415" },
                { "President Roxas", "9416" },
                { "Tulunan", "9417" }
            };

            _addressData["Davao del Norte"] = new Dictionary<string, string>
            {
                { "Tagum City", "8100" },
                { "Panabo City", "8105" },
                { "Island Garden City of Samal", "8119" },
                { "Asuncion", "8101" },
                { "Braulio E. Dujali", "8102" },
                { "Carmen", "8103" },
                { "Kapalong", "8104" },
                { "New Corella", "8106" },
                { "San Isidro", "8107" },
                { "Santo Tomas", "8108" },
                { "Talaingod", "8109" }
            };

            _addressData["Davao del Sur"] = new Dictionary<string, string>
            {
                { "Digos City", "8002" },
                { "Davao City", "8000" },
                { "Bansalan", "8005" },
                { "Hagonoy", "8006" },
                { "Kiblawan", "8007" },
                { "Magsaysay", "8008" },
                { "Malalag", "8009" },
                { "Matanao", "8003" },
                { "Padada", "8004" },
                { "Santa Cruz", "8001" },
                { "Sulop", "8010" }
            };

            _addressData["Davao Oriental"] = new Dictionary<string, string>
            {
                { "Mati City", "8200" },
                { "Baganga", "8201" },
                { "Banaybanay", "8202" },
                { "Boston", "8203" },
                { "Caraga", "8204" },
                { "Cateel", "8205" },
                { "Governor Generoso", "8206" },
                { "Lupon", "8207" },
                { "Manay", "8208" },
                { "San Isidro", "8209" },
                { "Tarragona", "8210" }
            };

            _addressData["Dinagat Islands"] = new Dictionary<string, string>
            {
                { "San Jose", "8411" },
                { "Basilisa", "8412" },
                { "Cagdianao", "8413" },
                { "Dinagat", "8414" },
                { "Libjo", "8415" },
                { "Loreto", "8416" },
                { "Tubajon", "8417" }
            };

            _addressData["Eastern Samar"] = new Dictionary<string, string>
            {
                { "Borongan City", "6800" },
                { "Arteche", "6801" },
                { "Balangiga", "6802" },
                { "Balangkayan", "6803" },
                { "Can-avid", "6804" },
                { "Dolores", "6805" },
                { "General MacArthur", "6806" },
                { "Giporlos", "6807" },
                { "Guiuan", "6808" },
                { "Hernani", "6809" },
                { "Jipapad", "6810" },
                { "Lawaan", "6811" },
                { "Llorente", "6812" },
                { "Maslog", "6813" },
                { "Maydolong", "6814" },
                { "Mercedes", "6815" },
                { "Oras", "6816" },
                { "Quinapondan", "6817" },
                { "Salcedo", "6818" },
                { "San Julian", "6819" },
                { "San Policarpo", "6820" },
                { "Sulat", "6821" },
                { "Taft", "6822" }
            };

            _addressData["Guimaras"] = new Dictionary<string, string>
            {
                { "Jordan", "5045" },
                { "Buenavista", "5046" },
                { "Nueva Valencia", "5047" },
                { "San Lorenzo", "5048" },
                { "Sibunag", "5049" }
            };

            _addressData["Ifugao"] = new Dictionary<string, string>
            {
                { "Lagawe", "3600" },
                { "Aguinaldo", "3601" },
                { "Alfonso Lista", "3602" },
                { "Asipulo", "3603" },
                { "Banaue", "3604" },
                { "Hingyon", "3605" },
                { "Hungduan", "3606" },
                { "Kiangan", "3607" },
                { "Lamut", "3608" },
                { "Mayoyao", "3609" },
                { "Tinoc", "3610" }
            };

            _addressData["Ilocos Norte"] = new Dictionary<string, string>
            {
                { "Laoag City", "2900" },
                { "Batac City", "2906" },
                { "Adams", "2901" },
                { "Bacarra", "2902" },
                { "Badoc", "2903" },
                { "Bangui", "2904" },
                { "Banna", "2905" },
                { "Burgos", "2907" },
                { "Carasi", "2908" },
                { "Currimao", "2909" },
                { "Dingras", "2910" },
                { "Dumalneg", "2911" },
                { "Marcos", "2912" },
                { "Nueva Era", "2913" },
                { "Pagudpud", "2914" },
                { "Paoay", "2915" },
                { "Pasuquin", "2916" },
                { "Piddig", "2917" },
                { "Pinili", "2918" },
                { "San Nicolas", "2919" },
                { "Sarrat", "2920" },
                { "Solsona", "2921" },
                { "Vintar", "2922" }
            };

            _addressData["Ilocos Sur"] = new Dictionary<string, string>
            {
                { "Vigan City", "2700" },
                { "Candon City", "2710" },
                { "Alilem", "2701" },
                { "Banayoyo", "2702" },
                { "Bantay", "2727" },
                { "Burgos", "2703" },
                { "Cabugao", "2728" },
                { "Caoayan", "2729" },
                { "Cervantes", "2704" },
                { "Galimuyod", "2705" },
                { "Gregorio del Pilar", "2706" },
                { "Lidlidda", "2707" },
                { "Magsingal", "2730" },
                { "Nagbukel", "2708" },
                { "Narvacan", "2709" },
                { "Quirino", "2711" },
                { "Salcedo", "2712" },
                { "San Emilio", "2713" },
                { "San Esteban", "2714" },
                { "San Ildefonso", "2715" },
                { "San Juan", "2731" },
                { "San Vicente", "2732" },
                { "Santa", "2733" },
                { "Santa Catalina", "2716" },
                { "Santa Cruz", "2717" },
                { "Santa Lucia", "2734" },
                { "Santa Maria", "2735" },
                { "Santiago", "2718" },
                { "Santo Domingo", "2736" },
                { "Sigay", "2719" },
                { "Sinait", "2737" },
                { "Sugpon", "2720" },
                { "Suyo", "2721" },
                { "Tagudin", "2722" }
            };

            _addressData["Iloilo"] = new Dictionary<string, string>
            {
                { "Iloilo City", "5000" },
                { "Passi City", "5037" },
                { "Ajuy", "5020" },
                { "Alimodian", "5001" },
                { "Anilao", "5021" },
                { "Badiangan", "5002" },
                { "Balasan", "5022" },
                { "Banate", "5023" },
                { "Barotac Nuevo", "5003" },
                { "Barotac Viejo", "5024" },
                { "Batad", "5025" },
                { "Bingawan", "5004" },
                { "Cabatuan", "5005" },
                { "Calinog", "5006" },
                { "Carles", "5026" },
                { "Concepcion", "5027" },
                { "Cuartero", "5028" },
                { "Dingle", "5007" },
                { "Dueñas", "5008" },
                { "Dumangas", "5009" },
                { "Estancia", "5029" },
                { "Guimbal", "5010" },
                { "Igbaras", "5011" },
                { "Janiuay", "5012" },
                { "Lambunao", "5013" },
                { "Leganes", "5014" },
                { "Lemery", "5030" },
                { "Leon", "5015" },
                { "Maasin", "5016" },
                { "Miagao", "5017" },
                { "Mina", "5018" },
                { "New Lucena", "5019" },
                { "Oton", "5020" },
                { "Pavia", "5001" },
                { "Pototan", "5002" },
                { "San Dionisio", "5031" },
                { "San Enrique", "5032" },
                { "San Joaquin", "5033" },
                { "San Miguel", "5003" },
                { "San Rafael", "5034" },
                { "Santa Barbara", "5002" },
                { "Sara", "5035" },
                { "Tigbauan", "5021" },
                { "Tubungan", "5022" },
                { "Zarraga", "5023" }
            };

            _addressData["Isabela"] = new Dictionary<string, string>
            {
                { "Ilagan City", "3300" },
                { "Cauayan City", "3305" },
                { "Santiago City", "3311" },
                { "Alicia", "3301" },
                { "Angadanan", "3302" },
                { "Aurora", "3303" },
                { "Benito Soliven", "3304" },
                { "Burgos", "3306" },
                { "Cabagan", "3307" },
                { "Cabatuan", "3308" },
                { "Cordon", "3309" },
                { "Delfin Albano", "3310" },
                { "Dinapigue", "3312" },
                { "Divilacan", "3313" },
                { "Echague", "3314" },
                { "Gamu", "3315" },
                { "Jones", "3316" },
                { "Luna", "3317" },
                { "Maconacon", "3318" },
                { "Mallig", "3319" },
                { "Naguilian", "3320" },
                { "Palanan", "3321" },
                { "Quezon", "3322" },
                { "Quirino", "3323" },
                { "Ramon", "3324" },
                { "Reina Mercedes", "3325" },
                { "Roxas", "3326" },
                { "San Agustin", "3327" },
                { "San Guillermo", "3328" },
                { "San Isidro", "3329" },
                { "San Manuel", "3330" },
                { "San Mariano", "3331" },
                { "San Mateo", "3332" },
                { "San Pablo", "3333" },
                { "Santa Maria", "3334" },
                { "Santo Tomas", "3335" },
                { "Tumauini", "3336" }
            };

            _addressData["Kalinga"] = new Dictionary<string, string>
            {
                { "Tabuk City", "3800" },
                { "Balbalan", "3801" },
                { "Lubuagan", "3802" },
                { "Pasil", "3803" },
                { "Pinukpuk", "3804" },
                { "Rizal", "3805" },
                { "Tanudan", "3806" },
                { "Tinglayan", "3807" }
            };

            _addressData["La Union"] = new Dictionary<string, string>
            {
                { "San Fernando City", "2500" },
                { "Agoo", "2504" },
                { "Aringay", "2506" },
                { "Bacnotan", "2515" },
                { "Bagulin", "2501" },
                { "Balaoan", "2516" },
                { "Bangar", "2517" },
                { "Bauang", "2501" },
                { "Burgos", "2518" },
                { "Caba", "2502" },
                { "Luna", "2519" },
                { "Naguilian", "2505" },
                { "Pugo", "2503" },
                { "Rosario", "2508" },
                { "San Gabriel", "2520" },
                { "San Juan", "2514" },
                { "Santo Tomas", "2511" },
                { "Santol", "2521" },
                { "Sudipen", "2522" },
                { "Tubao", "2523" }
            };

            _addressData["Laguna"] = new Dictionary<string, string>
            {
                { "Calamba City", "4027" },
                { "San Pablo City", "4000" },
                { "Santa Rosa City", "4026" },
                { "Biñan City", "4024" },
                { "Alaminos", "4001" },
                { "Bay", "4033" },
                { "Cabuyao", "4025" },
                { "Calauan", "4028" },
                { "Cavinti", "4013" },
                { "Famy", "4014" },
                { "Kalayaan", "4015" },
                { "Liliw", "4004" },
                { "Los Baños", "4030" },
                { "Luisiana", "4016" },
                { "Lumban", "4014" },
                { "Mabitac", "4017" },
                { "Magdalena", "4007" },
                { "Majayjay", "4018" },
                { "Nagcarlan", "4002" },
                { "Paete", "4015" },
                { "Pagsanjan", "4008" },
                { "Pakil", "4019" },
                { "Pangil", "4020" },
                { "Pila", "4009" },
                { "Rizal", "4021" },
                { "San Pedro", "4023" },
                { "Santa Cruz", "4009" },
                { "Santa Maria", "4022" },
                { "Siniloan", "4024" },
                { "Victoria", "4003" }
            };

            _addressData["Lanao del Norte"] = new Dictionary<string, string>
            {
                { "Iligan City", "9200" },
                { "Bacolod", "9201" },
                { "Baloi", "9202" },
                { "Baroy", "9203" },
                { "Kapatagan", "9204" },
                { "Kauswagan", "9205" },
                { "Kolambugan", "9206" },
                { "Lala", "9207" },
                { "Linamon", "9208" },
                { "Magsaysay", "9209" },
                { "Maigo", "9210" },
                { "Matungao", "9211" },
                { "Munai", "9212" },
                { "Nunungan", "9213" },
                { "Pantao Ragat", "9214" },
                { "Pantar", "9215" },
                { "Poona Piagapo", "9216" },
                { "Salvador", "9217" },
                { "Sapad", "9218" },
                { "Sultan Naga Dimaporo", "9219" },
                { "Tagoloan", "9220" },
                { "Tangcal", "9221" },
                { "Tubod", "9222" }
            };

            _addressData["Lanao del Sur"] = new Dictionary<string, string>
            {
                { "Marawi City", "9700" },
                { "Amai Manabilang", "9701" },
                { "Bacolod-Kalawi", "9702" },
                { "Balabagan", "9703" },
                { "Balindong", "9704" },
                { "Bayang", "9705" },
                { "Binidayan", "9706" },
                { "Buadiposo-Buntong", "9707" },
                { "Bubong", "9708" },
                { "Butig", "9709" },
                { "Calanogas", "9710" },
                { "Ditsaan-Ramain", "9711" },
                { "Ganassi", "9712" },
                { "Kapai", "9713" },
                { "Kapatagan", "9714" },
                { "Lumba-Bayabao", "9715" },
                { "Lumbaca-Unayan", "9716" },
                { "Lumbatan", "9717" },
                { "Lumbayanague", "9718" },
                { "Madalum", "9719" },
                { "Madamba", "9720" },
                { "Maguing", "9721" },
                { "Malabang", "9722" },
                { "Marantao", "9723" },
                { "Marogong", "9724" },
                { "Masiu", "9725" },
                { "Mulondo", "9726" },
                { "Pagayawan", "9727" },
                { "Piagapo", "9728" },
                { "Picong", "9729" },
                { "Poona Bayabao", "9730" },
                { "Pualas", "9731" },
                { "Saguiaran", "9732" },
                { "Sultan Dumalondong", "9733" },
                { "Tagoloan II", "9734" },
                { "Tamparan", "9735" },
                { "Taraka", "9736" },
                { "Tubaran", "9737" },
                { "Tugaya", "9738" },
                { "Wao", "9739" }
            };

            _addressData["Leyte"] = new Dictionary<string, string>
            {
                { "Tacloban City", "6500" },
                { "Ormoc City", "6541" },
                { "Abuyog", "6501" },
                { "Alangalang", "6502" },
                { "Albuera", "6503" },
                { "Babatngon", "6504" },
                { "Barugo", "6505" },
                { "Bato", "6506" },
                { "Baybay City", "6521" },
                { "Burauen", "6508" },
                { "Calubian", "6509" },
                { "Capoocan", "6510" },
                { "Carigara", "6511" },
                { "Dagami", "6512" },
                { "Dulag", "6513" },
                { "Hilongos", "6524" },
                { "Hindang", "6525" },
                { "Inopacan", "6526" },
                { "Isabel", "6527" },
                { "Jaro", "6528" },
                { "Javier", "6514" },
                { "Julita", "6515" },
                { "Kananga", "6529" },
                { "La Paz", "6516" },
                { "Leyte", "6517" },
                { "MacArthur", "6518" },
                { "Mahaplag", "6530" },
                { "Matag-ob", "6531" },
                { "Matalom", "6532" },
                { "Mayorga", "6519" },
                { "Merida", "6533" },
                { "Palo", "6501" },
                { "Palompon", "6534" },
                { "Pastrana", "6520" },
                { "San Isidro", "6535" },
                { "San Miguel", "6536" },
                { "Santa Fe", "6521" },
                { "Tabango", "6537" },
                { "Tabontabon", "6522" },
                { "Tanauan", "6523" },
                { "Tolosa", "6524" },
                { "Tunga", "6525" },
                { "Villaba", "6538" }
            };

            _addressData["Maguindanao"] = new Dictionary<string, string>
            {
                { "Cotabato City", "9600" },
                { "Ampatuan", "9601" },
                { "Barira", "9602" },
                { "Buldon", "9603" },
                { "Buluan", "9604" },
                { "Datu Abdulah Sangki", "9605" },
                { "Datu Anggal Midtimbang", "9606" },
                { "Datu Blah T. Sinsuat", "9607" },
                { "Datu Hoffer Ampatuan", "9608" },
                { "Datu Montawal", "9609" },
                { "Datu Odin Sinsuat", "9610" },
                { "Datu Paglas", "9611" },
                { "Datu Piang", "9612" },
                { "Datu Salibo", "9613" },
                { "Datu Saudi-Ampatuan", "9614" },
                { "Datu Unsay", "9615" },
                { "General Salipada K. Pendatun", "9616" },
                { "Guindulungan", "9617" },
                { "Kabuntalan", "9618" },
                { "Mamasapano", "9619" },
                { "Mangudadatu", "9620" },
                { "Matanog", "9621" },
                { "Northern Kabuntalan", "9622" },
                { "Pagalungan", "9623" },
                { "Paglat", "9624" },
                { "Pandag", "9625" },
                { "Parang", "9626" },
                { "Rajah Buayan", "9627" },
                { "Shariff Aguak", "9628" },
                { "Shariff Saydona Mustapha", "9629" },
                { "South Upi", "9630" },
                { "Sultan Kudarat", "9631" },
                { "Sultan Mastura", "9632" },
                { "Sultan sa Barongis", "9633" },
                { "Sultan Sumagka", "9634" },
                { "Talayan", "9635" },
                { "Talitay", "9636" },
                { "Upi", "9637" }
            };

            _addressData["Marinduque"] = new Dictionary<string, string>
            {
                { "Boac", "4900" },
                { "Buenavista", "4901" },
                { "Gasan", "4905" },
                { "Mogpog", "4902" },
                { "Santa Cruz", "4903" },
                { "Torrijos", "4904" }
            };

            _addressData["Masbate"] = new Dictionary<string, string>
            {
                { "Masbate City", "5400" },
                { "Aroroy", "5401" },
                { "Baleno", "5402" },
                { "Balud", "5403" },
                { "Batuan", "5404" },
                { "Cataingan", "5405" },
                { "Cawayan", "5406" },
                { "Claveria", "5407" },
                { "Dimasalang", "5408" },
                { "Esperanza", "5409" },
                { "Mandaon", "5410" },
                { "Milagros", "5411" },
                { "Mobo", "5412" },
                { "Monreal", "5413" },
                { "Palanas", "5414" },
                { "Pio V. Corpuz", "5415" },
                { "Placer", "5416" },
                { "San Fernando", "5417" },
                { "San Jacinto", "5418" },
                { "San Pascual", "5419" },
                { "Uson", "5420" }
            };

            _addressData["Metro Manila"] = new Dictionary<string, string>
            {
                { "Manila", "1000" },
                { "Quezon City", "1100" },
                { "Caloocan", "1400" },
                { "Las Piñas", "1740" },
                { "Makati", "1200" },
                { "Malabon", "1470" },
                { "Mandaluyong", "1550" },
                { "Marikina", "1800" },
                { "Muntinlupa", "1770" },
                { "Navotas", "1485" },
                { "Parañaque", "1700" },
                { "Pasay", "1300" },
                { "Pasig", "1600" },
                { "Pateros", "1620" },
                { "San Juan", "1500" },
                { "Taguig", "1630" },
                { "Valenzuela", "1440" }
            };

            _addressData["Misamis Occidental"] = new Dictionary<string, string>
            {
                { "Oroquieta City", "7200" },
                { "Ozamiz City", "7200" },
                { "Tangub City", "7214" },
                { "Aloran", "7201" },
                { "Baliangao", "7202" },
                { "Bonifacio", "7203" },
                { "Calamba", "7204" },
                { "Clarin", "7205" },
                { "Concepcion", "7206" },
                { "Don Victoriano Chiongbian", "7207" },
                { "Jimenez", "7208" },
                { "Lopez Jaena", "7209" },
                { "Panaon", "7210" },
                { "Plaridel", "7211" },
                { "Sapang Dalaga", "7212" },
                { "Sinacaban", "7213" },
                { "Tudela", "7215" }
            };

            _addressData["Misamis Oriental"] = new Dictionary<string, string>
            {
                { "Cagayan de Oro City", "9000" },
                { "Gingoog City", "9014" },
                { "Alubijid", "9001" },
                { "Balingasag", "9002" },
                { "Balingoan", "9003" },
                { "Binuangan", "9004" },
                { "Claveria", "9005" },
                { "El Salvador City", "9006" },
                { "Gitagum", "9007" },
                { "Initao", "9008" },
                { "Jasaan", "9009" },
                { "Kinoguitan", "9010" },
                { "Lagonglong", "9011" },
                { "Laguindingan", "9012" },
                { "Libertad", "9013" },
                { "Lugait", "9015" },
                { "Magsaysay", "9016" },
                { "Manticao", "9017" },
                { "Medina", "9018" },
                { "Naawan", "9019" },
                { "Opol", "9020" },
                { "Salay", "9021" },
                { "Sugbongcogon", "9022" },
                { "Tagoloan", "9023" },
                { "Talisayan", "9024" },
                { "Villanueva", "9025" }
            };

            _addressData["Mountain Province"] = new Dictionary<string, string>
            {
                { "Bontoc", "2616" },
                { "Barlig", "2617" },
                { "Bauko", "2618" },
                { "Besao", "2619" },
                { "Natonin", "2620" },
                { "Paracelis", "2621" },
                { "Sabangan", "2622" },
                { "Sadanga", "2623" },
                { "Sagada", "2624" },
                { "Tadian", "2625" }
            };

            _addressData["Negros Occidental"] = new Dictionary<string, string>
            {
                { "Bacolod City", "6100" },
                { "Bago City", "6101" },
                { "Cadiz City", "6121" },
                { "Escalante City", "6124" },
                { "Himamaylan City", "6108" },
                { "Kabankalan City", "6111" },
                { "La Carlota City", "6130" },
                { "Sagay City", "6122" },
                { "San Carlos City", "6127" },
                { "Silay City", "6116" },
                { "Sipalay City", "6113" },
                { "Talisay City", "6115" },
                { "Victorias City", "6119" },
                { "Binalbagan", "6107" },
                { "Calatrava", "6126" },
                { "Candoni", "6109" },
                { "Cauayan", "6112" },
                { "Enrique B. Magalona", "6118" },
                { "Hinigaran", "6106" },
                { "Hinoba-an", "6114" },
                { "Ilog", "6105" },
                { "Isabela", "6128" },
                { "La Castellana", "6131" },
                { "Manapla", "6120" },
                { "Moises Padilla", "6132" },
                { "Murcia", "6129" },
                { "Pontevedra", "6105" },
                { "Pulupandan", "6102" },
                { "Salvador Benedicto", "6133" },
                { "San Enrique", "6110" },
                { "Toboso", "6123" },
                { "Valladolid", "6103" }
            };

            _addressData["Negros Oriental"] = new Dictionary<string, string>
            {
                { "Dumaguete City", "6200" },
                { "Bais City", "6206" },
                { "Bayawan City", "6221" },
                { "Canlaon City", "6223" },
                { "Guihulngan City", "6214" },
                { "Tanjay City", "6204" },
                { "Amlan", "6201" },
                { "Ayungon", "6202" },
                { "Bacong", "6216" },
                { "Basay", "6217" },
                { "Bindoy", "6218" },
                { "Dauin", "6219" },
                { "Jimalalud", "6207" },
                { "La Libertad", "6208" },
                { "Mabinay", "6209" },
                { "Manjuyod", "6205" },
                { "Pamplona", "6210" },
                { "San Jose", "6222" },
                { "Santa Catalina", "6211" },
                { "Siaton", "6220" },
                { "Sibulan", "6203" },
                { "Tayasan", "6212" },
                { "Valencia", "6215" },
                { "Vallehermoso", "6213" },
                { "Zamboanguita", "6224" }
            };

            _addressData["Northern Samar"] = new Dictionary<string, string>
            {
                { "Catarman", "6400" },
                { "Allen", "6401" },
                { "Biri", "6402" },
                { "Bobon", "6403" },
                { "Capul", "6404" },
                { "Catubig", "6405" },
                { "Gamay", "6406" },
                { "Laoang", "6407" },
                { "Lapinig", "6408" },
                { "Las Navas", "6409" },
                { "Lavezares", "6410" },
                { "Lope de Vega", "6411" },
                { "Mapanas", "6412" },
                { "Mondragon", "6413" },
                { "Palapag", "6414" },
                { "Pambujan", "6415" },
                { "Rosario", "6416" },
                { "San Antonio", "6417" },
                { "San Isidro", "6418" },
                { "San Jose", "6419" },
                { "San Roque", "6420" },
                { "San Vicente", "6421" },
                { "Silvino Lobos", "6422" },
                { "Victoria", "6423" }
            };

            _addressData["Nueva Ecija"] = new Dictionary<string, string>
            {
                { "Palayan City", "3132" },
                { "Cabanatuan City", "3100" },
                { "Gapan City", "3105" },
                { "San Jose City", "3121" },
                { "Science City of Muñoz", "3119" },
                { "Aliaga", "3111" },
                { "Bongabon", "3128" },
                { "Cabiao", "3107" },
                { "Carranglan", "3130" },
                { "Cuyapo", "3117" },
                { "Gabaldon", "3131" },
                { "General Mamerto Natividad", "3109" },
                { "General Tinio", "3103" },
                { "Guimba", "3118" },
                { "Jaen", "3113" },
                { "Laur", "3114" },
                { "Licab", "3120" },
                { "Llanera", "3115" },
                { "Lupao", "3116" },
                { "Nampicuan", "3122" },
                { "Pantabangan", "3129" },
                { "Peñaranda", "3123" },
                { "Quezon", "3108" },
                { "Rizal", "3124" },
                { "San Antonio", "3106" },
                { "San Isidro", "3125" },
                { "San Leonardo", "3110" },
                { "Santa Rosa", "3126" },
                { "Santo Domingo", "3127" },
                { "Talavera", "3112" },
                { "Talugtug", "3133" },
                { "Zaragoza", "3101" }
            };

            _addressData["Nueva Vizcaya"] = new Dictionary<string, string>
            {
                { "Bayombong", "3700" },
                { "Alfonso Castañeda", "3701" },
                { "Ambaguio", "3702" },
                { "Aritao", "3703" },
                { "Bagabag", "3704" },
                { "Bambang", "3705" },
                { "Diadi", "3706" },
                { "Dupax del Norte", "3707" },
                { "Dupax del Sur", "3708" },
                { "Kasibu", "3709" },
                { "Kayapa", "3710" },
                { "Quezon", "3711" },
                { "Santa Fe", "3712" },
                { "Solano", "3713" },
                { "Villaverde", "3714" }
            };

            _addressData["Occidental Mindoro"] = new Dictionary<string, string>
            {
                { "Mamburao", "5100" },
                { "Abra de Ilog", "5101" },
                { "Calintaan", "5102" },
                { "Looc", "5103" },
                { "Lubang", "5104" },
                { "Magsaysay", "5105" },
                { "Paluan", "5106" },
                { "Rizal", "5107" },
                { "Sablayan", "5108" },
                { "San Jose", "5109" },
                { "Santa Cruz", "5110" }
            };

            _addressData["Oriental Mindoro"] = new Dictionary<string, string>
            {
                { "Calapan City", "5200" },
                { "Baco", "5201" },
                { "Bansud", "5202" },
                { "Bongabong", "5203" },
                { "Bulalacao", "5204" },
                { "Gloria", "5205" },
                { "Mansalay", "5206" },
                { "Naujan", "5207" },
                { "Pinamalayan", "5208" },
                { "Pola", "5209" },
                { "Puerto Galera", "5210" },
                { "Roxas", "5211" },
                { "San Teodoro", "5212" },
                { "Socorro", "5213" },
                { "Victoria", "5214" }
            };

            _addressData["Palawan"] = new Dictionary<string, string>
            {
                { "Puerto Princesa City", "5300" },
                { "Aborlan", "5302" },
                { "Agutaya", "5318" },
                { "Araceli", "5319" },
                { "Balabac", "5307" },
                { "Bataraza", "5306" },
                { "Brooke's Point", "5305" },
                { "Busuanga", "5311" },
                { "Cagayancillo", "5320" },
                { "Coron", "5316" },
                { "Culion", "5315" },
                { "Cuyo", "5321" },
                { "Dumaran", "5322" },
                { "El Nido", "5313" },
                { "Kalayaan", "5323" },
                { "Linapacan", "5314" },
                { "Magsaysay", "5324" },
                { "Narra", "5303" },
                { "Quezon", "5304" },
                { "Rizal", "5301" },
                { "Roxas", "5308" },
                { "San Vicente", "5309" },
                { "Sofronio Española", "5310" },
                { "Taytay", "5312" }
            };

            _addressData["Pampanga"] = new Dictionary<string, string>
            {
                { "Angeles City", "2009" },
                { "San Fernando City", "2000" },
                { "Mabalacat City", "2010" },
                { "Apalit", "2016" },
                { "Arayat", "2012" },
                { "Bacolor", "2001" },
                { "Candaba", "2017" },
                { "Floridablanca", "2011" },
                { "Guagua", "2003" },
                { "Lubao", "2004" },
                { "Macabebe", "2018" },
                { "Magalang", "2013" },
                { "Masantol", "2019" },
                { "Mexico", "2021" },
                { "Minalin", "2005" },
                { "Porac", "2008" },
                { "Sasmuan", "2020" },
                { "Santa Ana", "2022" },
                { "Santa Rita", "2006" },
                { "Santo Tomas", "2015" }
            };

            _addressData["Pangasinan"] = new Dictionary<string, string>
            {
                { "Lingayen", "2401" },
                { "Alaminos City", "2404" },
                { "Dagupan City", "2400" },
                { "San Carlos City", "2420" },
                { "Urdaneta City", "2428" },
                { "Agno", "2408" },
                { "Aguilar", "2416" },
                { "Alcala", "2425" },
                { "Anda", "2405" },
                { "Asingan", "2439" },
                { "Balungao", "2442" },
                { "Bani", "2407" },
                { "Basista", "2421" },
                { "Bautista", "2443" },
                { "Bayambang", "2423" },
                { "Binalonan", "2436" },
                { "Binmaley", "2417" },
                { "Bolinao", "2406" },
                { "Bugallon", "2018" },
                { "Burgos", "2419" },
                { "Calasiao", "2013" },
                { "Dasol", "2411" },
                { "Infanta", "2412" },
                { "Labrador", "2410" },
                { "Laoac", "2437" },
                { "Mabini", "2440" },
                { "Malasiqui", "2421" },
                { "Manaoag", "2430" },
                { "Mangaldan", "2403" },
                { "Mangatarem", "2431" },
                { "Mapandan", "2432" },
                { "Natividad", "2441" },
                { "Pozorrubio", "2435" },
                { "Rosales", "2441" },
                { "San Fabian", "2433" },
                { "San Jacinto", "2444" },
                { "San Manuel", "2434" },
                { "San Nicolas", "2447" },
                { "San Quintin", "2445" },
                { "Santa Barbara", "2402" },
                { "Santa Maria", "2446" },
                { "Santo Tomas", "2426" },
                { "Sison", "2438" },
                { "Sual", "2409" },
                { "Tayug", "2448" },
                { "Umingan", "2449" },
                { "Urbiztondo", "2447" },
                { "Villasis", "2427" }
            };

            _addressData["Quezon"] = new Dictionary<string, string>
            {
                { "Lucena City", "4301" },
                { "Tayabas City", "4327" },
                { "Agdangan", "4304" },
                { "Alabat", "4302" },
                { "Atimonan", "4331" },
                { "Buenavista", "4320" },
                { "Burdeos", "4340" },
                { "Calauag", "4318" },
                { "Candelaria", "4323" },
                { "Catanauan", "4311" },
                { "Dolores", "4326" },
                { "General Luna", "4203" },
                { "General Nakar", "4338" },
                { "Guinayangan", "4319" },
                { "Gumaca", "4307" },
                { "Infanta", "4336" },
                { "Jomalig", "4339" },
                { "Lopez", "4316" },
                { "Lucban", "4328" },
                { "Macalelon", "4313" },
                { "Mauban", "4330" },
                { "Mulanay", "4312" },
                { "Padre Burgos", "4303" },
                { "Pagbilao", "4305" },
                { "Panukulan", "4341" },
                { "Patnanungan", "4306" },
                { "Perez", "4337" },
                { "Pitogo", "4308" },
                { "Plaridel", "4309" },
                { "Polillo", "4335" },
                { "Quezon", "4321" },
                { "Real", "4335" },
                { "Sampaloc", "4329" },
                { "San Andres", "4310" },
                { "San Antonio", "4322" },
                { "San Francisco", "4314" },
                { "San Narciso", "4324" },
                { "Sariaya", "4325" },
                { "Tagkawayan", "4317" },
                { "Tiaong", "4325" },
                { "Unisan", "4315" }
            };

            _addressData["Quirino"] = new Dictionary<string, string>
            {
                { "Cabarroguis", "3400" },
                { "Aglipay", "3401" },
                { "Diffun", "3402" },
                { "Maddela", "3403" },
                { "Nagtipunan", "3404" },
                { "Saguday", "3405" }
            };

            _addressData["Rizal"] = new Dictionary<string, string>
            {
                { "Antipolo City", "1870" },
                { "Angono", "1930" },
                { "Baras", "1969" },
                { "Binangonan", "1940" },
                { "Cainta", "1900" },
                { "Cardona", "1950" },
                { "Jalajala", "1965" },
                { "Morong", "1960" },
                { "Pililla", "1910" },
                { "Rodriguez", "1860" },
                { "San Mateo", "1850" },
                { "Tanay", "1980" },
                { "Taytay", "1920" },
                { "Teresa", "1880" }
            };

            _addressData["Romblon"] = new Dictionary<string, string>
            {
                { "Romblon", "5500" },
                { "Alcantara", "5501" },
                { "Banton", "5502" },
                { "Cajidiocan", "5503" },
                { "Calatrava", "5504" },
                { "Concepcion", "5505" },
                { "Corcuera", "5506" },
                { "Ferrol", "5507" },
                { "Looc", "5508" },
                { "Magdiwang", "5509" },
                { "Odiongan", "5510" },
                { "San Agustin", "5511" },
                { "San Andres", "5512" },
                { "San Fernando", "5513" },
                { "San Jose", "5514" },
                { "Santa Fe", "5515" },
                { "Santa Maria", "5516" }
            };

            _addressData["Samar"] = new Dictionary<string, string>
            {
                { "Catbalogan City", "6700" },
                { "Calbayog City", "6710" },
                { "Almagro", "6701" },
                { "Basey", "6720" },
                { "Calbiga", "6702" },
                { "Daram", "6721" },
                { "Gandara", "6703" },
                { "Hinabangan", "6704" },
                { "Jiabong", "6705" },
                { "Marabut", "6722" },
                { "Matuguinao", "6706" },
                { "Motiong", "6707" },
                { "Pagsanghan", "6708" },
                { "Paranas", "6709" },
                { "Pinabacdao", "6711" },
                { "San Jorge", "6723" },
                { "San Jose de Buan", "6712" },
                { "San Sebastian", "6713" },
                { "Santa Margarita", "6714" },
                { "Santa Rita", "6715" },
                { "Santo Niño", "6716" },
                { "Tagapul-an", "6717" },
                { "Talalora", "6718" },
                { "Tarangnan", "6719" },
                { "Villareal", "6724" },
                { "Zumarraga", "6725" }
            };

            _addressData["Sarangani"] = new Dictionary<string, string>
            {
                { "Alabel", "9501" },
                { "Glan", "9517" },
                { "Kiamba", "9502" },
                { "Maasim", "9503" },
                { "Maitum", "9504" },
                { "Malapatan", "9505" },
                { "Malungon", "9506" }
            };

            _addressData["Siquijor"] = new Dictionary<string, string>
            {
                { "Siquijor", "6227" },
                { "Enrique Villanueva", "6228" },
                { "Larena", "6229" },
                { "Lazi", "6230" },
                { "Maria", "6231" },
                { "San Juan", "6232" }
            };

            _addressData["Sorsogon"] = new Dictionary<string, string>
            {
                { "Sorsogon City", "4700" },
                { "Barcelona", "4716" },
                { "Bulan", "4706" },
                { "Bulusan", "4704" },
                { "Casiguran", "4705" },
                { "Castilla", "4701" },
                { "Donsol", "4715" },
                { "Gubat", "4710" },
                { "Irosin", "4707" },
                { "Juban", "4703" },
                { "Magallanes", "4706" },
                { "Matnog", "4717" },
                { "Pilar", "4702" },
                { "Prieto Diaz", "4712" },
                { "Santa Magdalena", "4713" }
            };

            _addressData["South Cotabato"] = new Dictionary<string, string>
            {
                { "General Santos City", "9500" },
                { "Koronadal City", "9506" },
                { "Banga", "9511" },
                { "Lake Sebu", "9507" },
                { "Norala", "9508" },
                { "Polomolok", "9504" },
                { "Santo Niño", "9512" },
                { "Surallah", "9512" },
                { "T'Boli", "9513" },
                { "Tampakan", "9514" },
                { "Tantangan", "9515" },
                { "Tupi", "9505" }
            };

            _addressData["Southern Leyte"] = new Dictionary<string, string>
            {
                { "Maasin City", "6600" },
                { "Anahawan", "6601" },
                { "Bontoc", "6602" },
                { "Hinunangan", "6603" },
                { "Hinundayan", "6604" },
                { "Libagon", "6605" },
                { "Liloan", "6606" },
                { "Limasawa", "6607" },
                { "Macrohon", "6608" },
                { "Malitbog", "6609" },
                { "Padre Burgos", "6610" },
                { "Pintuyan", "6611" },
                { "Saint Bernard", "6612" },
                { "San Francisco", "6613" },
                { "San Juan", "6614" },
                { "San Ricardo", "6615" },
                { "Silago", "6616" },
                { "Sogod", "6617" },
                { "Tomas Oppus", "6618" }
            };

            _addressData["Sultan Kudarat"] = new Dictionary<string, string>
            {
                { "Isulan", "9805" },
                { "Tacurong City", "9800" },
                { "Bagumbayan", "9801" },
                { "Columbio", "9802" },
                { "Esperanza", "9803" },
                { "Kalamansig", "9804" },
                { "Lebak", "9806" },
                { "Lutayan", "9807" },
                { "Lambayong", "9808" },
                { "Palimbang", "9809" },
                { "President Quirino", "9810" },
                { "Senator Ninoy Aquino", "9811" }
            };

            _addressData["Sulu"] = new Dictionary<string, string>
            {
                { "Jolo", "7400" },
                { "Banguingui", "7401" },
                { "Hadji Panglima Tahil", "7402" },
                { "Indanan", "7403" },
                { "Kalingalan Caluang", "7404" },
                { "Lugus", "7405" },
                { "Luuk", "7406" },
                { "Maimbung", "7407" },
                { "Old Panamao", "7408" },
                { "Omar", "7409" },
                { "Pandami", "7410" },
                { "Panglima Estino", "7411" },
                { "Pangutaran", "7412" },
                { "Parang", "7413" },
                { "Pata", "7414" },
                { "Patikul", "7415" },
                { "Siasi", "7416" },
                { "Talipao", "7417" },
                { "Tapul", "7418" },
                { "Tongkil", "7419" }
            };

            _addressData["Surigao del Norte"] = new Dictionary<string, string>
            {
                { "Surigao City", "8400" },
                { "Alegria", "8401" },
                { "Bacuag", "8402" },
                { "Burgos", "8403" },
                { "Claver", "8404" },
                { "Dapa", "8405" },
                { "Del Carmen", "8406" },
                { "General Luna", "8407" },
                { "Gigaquit", "8408" },
                { "Mainit", "8409" },
                { "Malimono", "8410" },
                { "Pilar", "8411" },
                { "Placer", "8412" },
                { "San Benito", "8413" },
                { "San Francisco", "8414" },
                { "San Isidro", "8415" },
                { "Santa Monica", "8416" },
                { "Sison", "8417" },
                { "Socorro", "8418" },
                { "Tagana-an", "8419" },
                { "Tubod", "8420" }
            };

            _addressData["Surigao del Sur"] = new Dictionary<string, string>
            {
                { "Tandag City", "8300" },
                { "Bislig City", "8311" },
                { "Barobo", "8301" },
                { "Bayabas", "8302" },
                { "Cagwait", "8303" },
                { "Cantilan", "8304" },
                { "Carmen", "8305" },
                { "Carrascal", "8306" },
                { "Cortes", "8307" },
                { "Hinatuan", "8308" },
                { "Lanuza", "8309" },
                { "Lianga", "8310" },
                { "Lingig", "8312" },
                { "Madrid", "8313" },
                { "Marihatag", "8314" },
                { "San Agustin", "8315" },
                { "San Miguel", "8316" },
                { "Tagbina", "8317" },
                { "Tago", "8318" }
            };

            _addressData["Tarlac"] = new Dictionary<string, string>
            {
                { "Tarlac City", "2300" },
                { "Anao", "2301" },
                { "Bamban", "2302" },
                { "Camiling", "2306" },
                { "Capas", "2315" },
                { "Concepcion", "2003" },
                { "Gerona", "2004" },
                { "La Paz", "2005" },
                { "Mayantoc", "2006" },
                { "Moncada", "2007" },
                { "Paniqui", "2008" },
                { "Pura", "2009" },
                { "Ramos", "2010" },
                { "San Clemente", "2011" },
                { "San Jose", "2012" },
                { "San Manuel", "2013" },
                { "Santa Ignacia", "2014" },
                { "Victoria", "2015" }
            };

            _addressData["Tawi-Tawi"] = new Dictionary<string, string>
            {
                { "Bongao", "7500" },
                { "Languyan", "7501" },
                { "Mapun", "7502" },
                { "Panglima Sugala", "7503" },
                { "Sapa-Sapa", "7504" },
                { "Sibutu", "7505" },
                { "Simunul", "7506" },
                { "Sitangkai", "7507" },
                { "South Ubian", "7508" },
                { "Tandubas", "7509" },
                { "Turtle Islands", "7510" }
            };

            _addressData["Zambales"] = new Dictionary<string, string>
            {
                { "Olongapo City", "2200" },
                { "Botolan", "2202" },
                { "Cabangan", "2203" },
                { "Candelaria", "2212" },
                { "Castillejos", "2208" },
                { "Iba", "2201" },
                { "Masinloc", "2211" },
                { "Palauig", "2204" },
                { "San Antonio", "2206" },
                { "San Felipe", "2207" },
                { "San Marcelino", "2209" },
                { "San Narciso", "2205" },
                { "Santa Cruz", "2210" },
                { "Subic", "2222" }
            };

            _addressData["Zamboanga del Norte"] = new Dictionary<string, string>
            {
                { "Dipolog City", "7100" },
                { "Dapitan City", "7101" },
                { "Baliguian", "7102" },
                { "Godod", "7103" },
                { "Gutalac", "7104" },
                { "Jose Dalman", "7105" },
                { "Kalawit", "7106" },
                { "Katipunan", "7107" },
                { "La Libertad", "7108" },
                { "Labason", "7109" },
                { "Leon B. Postigo", "7110" },
                { "Liloy", "7111" },
                { "Manukan", "7112" },
                { "Mutia", "7113" },
                { "Piñan", "7114" },
                { "Polanco", "7115" },
                { "President Manuel A. Roxas", "7116" },
                { "Rizal", "7117" },
                { "Salug", "7118" },
                { "Sergio Osmeña Sr.", "7119" },
                { "Siayan", "7120" },
                { "Sibuco", "7121" },
                { "Sibutad", "7122" },
                { "Sindangan", "7123" },
                { "Siocon", "7124" },
                { "Sirawai", "7125" },
                { "Tampilisan", "7126" }
            };

            _addressData["Zamboanga del Sur"] = new Dictionary<string, string>
            {
                { "Pagadian City", "7016" },
                { "Zamboanga City", "7000" },
                { "Aurora", "7020" },
                { "Bayog", "7011" },
                { "Dimataling", "7017" },
                { "Dinas", "7018" },
                { "Dumalinao", "7012" },
                { "Dumingag", "7013" },
                { "Guipos", "7021" },
                { "Josefina", "7022" },
                { "Kumalarang", "7014" },
                { "Labangan", "7015" },
                { "Lakewood", "7019" },
                { "Lapuyan", "7023" },
                { "Mahayag", "7024" },
                { "Margosatubig", "7025" },
                { "Midsalip", "7026" },
                { "Molave", "7027" },
                { "Pitogo", "7028" },
                { "Ramon Magsaysay", "7029" },
                { "San Miguel", "7030" },
                { "San Pablo", "7031" },
                { "Sominot", "7032" },
                { "Tabina", "7033" },
                { "Tambulig", "7034" },
                { "Tigbao", "7035" },
                { "Tukuran", "7036" },
                { "Vincenzo A. Sagun", "7037" }
            };

            _addressData["Zamboanga Sibugay"] = new Dictionary<string, string>
            {
                { "Ipil", "7001" },
                { "Alicia", "7002" },
                { "Buug", "7003" },
                { "Diplahan", "7004" },
                { "Imelda", "7005" },
                { "Kabasalan", "7006" },
                { "Mabuhay", "7007" },
                { "Malangas", "7008" },
                { "Naga", "7009" },
                { "Olutanga", "7010" },
                { "Payao", "7037" },
                { "Roseller Lim", "7038" },
                { "Siay", "7039" },
                { "Talusan", "7040" },
                { "Titay", "7041" },
                { "Tungawan", "7042" }
            };
        }

        public Dictionary<string, Dictionary<string, string>> GetAddressData()
        {
            return _addressData;
        }

        public List<string> GetProvinces()
        {
            return new List<string>(_addressData.Keys);
        }

        public List<string> GetCities(string province)
        {
            if (string.IsNullOrWhiteSpace(province) || !_addressData.ContainsKey(province))
                return new List<string>();

            return new List<string>(_addressData[province].Keys);
        }

        public string GetZipCode(string province, string city)
        {
            if (string.IsNullOrWhiteSpace(province) || string.IsNullOrWhiteSpace(city))
                return string.Empty;

            if (_addressData.ContainsKey(province) && _addressData[province].ContainsKey(city))
                return _addressData[province][city];

            return string.Empty;
        }
    }
}

