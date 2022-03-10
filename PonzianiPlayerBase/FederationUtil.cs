using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PonzianiPlayerBase
{
    public static class FederationUtil
    {
        static FederationUtil()
        {

            InitializeFifaIOC();
        }

        private static void InitializeFifaIOC()
        {
            string fedFifa = @"Algeria;ALG;ALG
Angola;ANG;ANG
Benin;BEN;BEN
Botswana;BOT;BOT
Burkina Faso;BFA;BUR
Burundi;BDI;BDI
Cameroon;CMR;CMR
Cape Verde Islands;CPV;CPV
Central African Republic;CTA;CAF
Chad;CHA;CHA
Comoros Islands;COM;COM
Congo;CGO;CGO
Congo DR (Zaire);COD;COD
Cote d’Ivoire;CIV;CIV
Djibouti;DJI;DJI
Egypt;EGY;EGY
Equatorial Guinea;EQG;GEQ
Eritrea;ERI;ERI
Ethiopia;ETH;ETH
Gabon;GAB;GAB
Gambia;GAM;GAM
Ghana;GHA;GHA
Guinea;GUI;GUI
Guinea-Bissau;GNB;GBS
Kenya;KEN;KEN
Lesotho;LES;LES
Liberia;LBR;LBR
Libya;LBY;LBA
Madagascar;MAD;MAD
Malawi;MWI;MAL
Mali;MLI;MLI
Mauritania;MTN;MTN
Mauritius;MRI;MRI
Morocco;MAR;MAR
Mozambique;MOZ;MOZ
Namibia;NAM;NAM
Niger;NIG;NIG
Nigeria;NGA;NGR
Rwanda;RWA;RWA
Sao Tome e Principe;STP;STP
Senegal;SEN;SEN
Seychelles;SEY;SEY
Sierra Leone;SLE;SLE
Somalia;SOM;SOM
South Africa;RSA;RSA
South Sudan;SSD;SSD
Sudan;SDN;SUD
Swaziland;SWZ;SWZ
Tanzania;TAN;TAN
Togo;TOG;TOG
Tunisia;TUN;TUN
Uganda;UGA;UGA
Zambia;ZAM;ZAM
Zimbabwe;ZIM;ZIM
Afghanistan;AFG;AFG
Australia;AUS;-----
Bahrain;BHR;BRN
Bangladesh;BAN;BAN
Bhutan;BHU;BHU
Brunei Darussalam;BRU;BRU
Cambodia;CAM;CAM
China PR;CHN;CHN
Chinese Taipei;TPE;TPE
East Timor;TLS;TLS
Guam;GUM;-----
Hong Kong;HKG;HKG
India;IND;IND
Indonesia;IDN;INA
Iran;IRN;IRI
Iraq;IRQ;IRQ
Japan;JPN;JPN
Jordan;JOR;JOR
Korea DPR;PRK;PRK
Korea Republic;KOR;KOR
Kuwait;KUW;KUW
Kyrgyzstan;KGZ;KGZ
Laos;LAO;LAO
Lebanon;LBN;LBN
Macao;MAC;-----
Malaysia;MAS;MAS
Maldives;MDV;MDV
Mongolia;MGL;MGL
Myanmar;MYA;MYA
Nepal;NEP;NEP
Oman;OMA;OMA
Pakistan;PAK;PAK
Palestine;PAL;PLE
Philippines;PHI;PHI
Qatar;QAT;QAT
Saudi Arabia;KSA;KSA
Singapore;SIN;SIN
Sri Lanka;SRI;SRI
Syria;SYR;SYR
Tajikistan;TJK;TJK
Thailand;THA;THA
Turkmenistan;TKM;TKM
United Arab Emirates;UAE;UAE
Uzbekistan;UZB;UZB
Vietnam ;VIE;VIE
Yemen;YEM;YEM
Albania;ALB;ALB
Andorra;AND;AND
Armenia;ARM;ARM
Austria;AUT;AUT
Azerbaijan;AZE;AZE
Belarus;BLR;BLR
Belgium;BEL;BEL
Bosnia Herzegovina;BIH;BIH
Bulgaria;BUL;BUL
Croatia;CRO;CRO
Cyprus;CYP;CYP
Czech Republic;CZE;TCH
Denmark;DEN;DEN
England;ENG;-----
Estonia;EST;EST
Faeroe Islands;FRO;-----
Finland;FIN;FIN
France;FRA;FRA
Georgia;GEO;GEO
Germany;GER;GER
Gibraltar;GIB;-----
Great Britain;-----;GBR
Greece;GRE;GRE
Holland;NED;NED
Hungary;HUN;HUN
Iceland;ISL;ISL
Israel;ISR;ISR
Italy;ITA;ITA
Kazakhstan;KAZ;KAZ
Kosovo;KVX;KOS
Latvia;LVA;LAT
Liechtenstein;LIE;LIE
Lithuania;LTU;LTU
Luxembourg;LUX;LUX
Macedonia FYR;MKD;MKD
Malta;MLT;MLT
Moldova;MDA;MDA
Monaco;-----;MON
Montenegro;MNE;MNE
Northern Ireland;NIR;-----
Norway;NOR;NOR
Poland;POL;POL
Portugal;POR;POR
Republic of Ireland;IRL;IRL
Romania;ROU;ROU
Russia;RUS;RUS
San Marino;SMR;SMR
Scotland;SCO;-----
Serbia;SRB;SRB
Slovakia;SVK;SVK
Slovenia;SVN;SLO
Spain;ESP;ESP
Sweden;SWE;SWE
Switzerland;SUI;SUI
Turkey;TUR;TUR
Ukraine;UKR;UKR
Wales;WAL;-----
Vatican;-----;VAT
Anguilla;AIA;-----
Antigua & Barbuda;ATG;ANT
Aruba;ARU;ARU
Bahamas;BAH;BAH
Barbados;BRB;BAR
Belize;BLZ;BIZ
Bermuda;BER;BER
British Virgin Islands;VGB;IVB
Canada;CAN;CAN
Cayman Islands;CAY;CAY
Costa Rica;CRC;CRC
Cuba;CUB;CUB
Curaçao;CUW;-----
Dominica;DMA;DMA
Dominican Republic;DOM;DOM
El Salvador;SLV;ESA
Grenada;GRN;GRN
Guatemala;GUA;GUA
Guyana;GUY;GUY
Haiti;HAI;HAI
Honduras;HON;HON
Jamaica;JAM;JAM
Mexico;MEX;MEX
Montserrat;MSR;-----
Nicaragua;NCA;NCA
Panama;PAN;PAN
Puerto Rico;PUR;PUR
St Kitts & Nevis;SKN;SKN
St Lucia;LCA;LCA
St Vincent & The Grenadines;VIN;VIN
Surinam;SUR;SUR
Trinidad & Tobago;TRI;TRI
Turks & Caicos Islands;TCA;-----
United States of America;USA;USA
United States Virgin Islands;VIR;ISV
American Samoa;ASA;ASA
Australia;-----;AUS
Cook Islands;COK;COK
Fiji;FIJ;FIJ
Guam;-----;GUM
Kiribati;-----;KIR
Marshall Islands;-----;MHL
Micronesia;-----;FSM
Nauru;-----;NRU
New Caledonia;NCL;-----
New Zealand;NZL;NZL
Palau;-----;PLW
Papua New Guinea;PNG;PNG
Samoa;SAM;SAM
Solomon Islands;SOL;SOL
Tahiti;TAH;-----
Tonga;TGA;TGA
Tuvalu;-----;TUV
Vanuatu;VAN;VAN
Argentina;ARG;ARG
Bolivia;BOL;BOL
Brazil;BRA;BRA
Chile;CHI;CHI
Colombia;COL;COL
Ecuador;ECU;ECU
Paraguay;PAR;PAR
Peru;PER;PER
Uruguay;URU;URU
Venezuela;VEN;VEN";
            string[] lines = fedFifa.Split(new String[] { "\r\n", "\n", "\r" }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            Federations = new();
            foreach (string line in lines)
            {
                string[] parts = line.Split(";");
                string key = parts[2][0] != '-' ? parts[2] : parts[1]; 
                if (!Federations.ContainsKey(key)) Federations.Add(key, parts[0]); 
            }
        }

        private static void InitializeIso()
        {
            string feds = "AC;AD;AE;AF;AG;AI;AL;AM;AN;AO;AP;AQ;AR;AS;AT;AU;AW;AX;AZ;BA;BB;BD;BE;BF;BG;BH;BI;BJ;BL;BM;BN;BO;BQ;BR;BS;BT;BU;BV;BW;BX;BY;BZ;CA;CC;CD;CF;CG;CH;CI;CK;CL;CM;CN;CO;CP;CQ;CR;CS;CT;CU;CV;CW;CX;CY;CZ;DD;DE;DG;DJ;DK;DM;DO;DY;DZ;EA;EC;EE;EF;EG;EH;EM;EP;ER;ES;ET;EU;EV;EW;EZ;FI;FJ;FK;FL;FM;FO;FQ;FR;FX;GA;GB;GC;GD;GE;GF;GG;GH;GI;GL;GM;GN;GP;GQ;GR;GS;GT;GU;GW;GY;HK;HM;HN;HR;HT;HU;HV;IB;IC;ID;IE;IL;IM;IN;IO;IQ;IR;IS;IT;JA;JE;JM;JO;JP;JT;KE;KG;KH;KI;KM;KN;KP;KR;KW;KY;KZ;LA;LB;LC;LF;LI;LK;LR;LS;LT;LU;LV;LY;MA;MC;MD;ME;MF;MG;MH;MI;MK;ML;MM;MN;MO;MP;MQ;MR;MS;MT;MU;MV;MW;MX;MY;MZ;NA;NC;NE;NF;NG;NH;NI;NL;NO;NP;NQ;NR;NT;NU;NZ;OA;OM;PA;PC;PE;PF;PG;PH;PI;PK;PL;PM;PN;PR;PS;PT;PU;PW;PY;PZ;QA;RA;RB;RC;RE;RH;RI;RL;RM;RN;RO;RP;RS;RU;RW;SA;SB;SC;SD;SE;SF;SG;SH;SI;SJ;SK;SL;SM;SN;SO;SR;SS;ST;SU;SV;SX;SY;SZ;TA;TC;TD;TF;TG;TH;TJ;TK;TL;TM;TN;TO;TP;TR;TT;TV;TW;TZ;UA;UG;UK;UM;UN;US;UY;UZ;VA;VC;VD;VE;VG;VI;VN;VU;WF;WG;WK;WL;WO;WS;WV;YD;YE;YT;YU;YV;ZA;ZM;ZR;ZW";
            string[] fed2 = feds.Split(";");
            Federations = new();
            foreach (string fed in fed2)
            {
                try
                {
                    RegionInfo ri = new(fed);
                    Federations.Add(ri.ThreeLetterISORegionName, ri.DisplayName);
                }
                catch
                {
                    continue;
                }
            }
        }

        public static Dictionary<string, string> Federations;

    }
}
