using System.Collections.Generic;

namespace CybersecurityChatbot.Models
{
    // This class represents a single chatbot response.
    // It stores keywords, possible replies, and a category for organisation.
    public class Response
    {
        // Keywords that the chatbot will look for in user input.
        public string[] Keywords { get; set; }

        // List of possible responses the chatbot can choose from randomly.
        public List<string> ResponseOptions { get; set; }

        // Category groups related responses and helps the bot remember the last topic.
        public string Category { get; set; }

        // Constructor used to create a new Response object.
        public Response(string[] keywords, List<string> responseOptions, string category = "General")
        {
            Keywords = keywords;
            ResponseOptions = responseOptions;
            Category = category;
        }
    }

    // This static class stores all predefined chatbot responses.
    // It acts like a simple knowledge base for the chatbot.
    public static class ResponseBank
    {
        // This method returns all chatbot responses.
        public static List<Response> GetResponses()
        {
            return new List<Response>
            {
                // Greeting responses
                new Response(
                    new[] { "hello", "hi", "hey", "greetings", "good morning", "good afternoon", "good evening", "howzit" },
                    new List<string>
                    {
                        "Hello! Welcome to the Cybersecurity Awareness Bot. I am ready to help you stay safe online today.",
                        "Hi there! I am your Cybersecurity Awareness Assistant. Ask me anything about online safety.",
                        "Hey! I am here to guide you through important cybersecurity tips and safe online habits."
                    },
                    "Greeting"
                ),

                // Asking how the bot is doing
                new Response(
                    new[] { "how are you", "how do you do", "are you okay", "how is it going" },
                    new List<string>
                    {
                        "I am doing great, thank you for asking. I am ready to help you learn how to stay safe online.",
                        "I am doing well. My goal is to help you understand cybersecurity in a simple and useful way."
                    },
                    "Greeting"
                ),

                // Bot purpose
                new Response(
                    new[] { "purpose", "what can you do", "what do you do", "your purpose", "what is your purpose", "why were you created", "who are you" },
                    new List<string>
                    {
                        "I am your Cybersecurity Awareness Assistant. I can help you with password safety, phishing, scams, safe browsing, malware, social engineering, suspicious links, online privacy, banking safety, WhatsApp scams, public Wi-Fi, and two-factor authentication.",
                        "My purpose is to educate users about cybersecurity threats and teach safe online practices in a simple conversation."
                    },
                    "Purpose"
                ),

                // Help topics
                new Response(
                    new[] { "what can i ask you about", "topics", "help", "help me", "more topics", "what else", "menu", "options", "list topics" },
                    new List<string>
                    {
                        "You can ask me about:\n" +
                        "• Password safety\n" +
                        "• Phishing and suspicious links\n" +
                        "• Malware, viruses, and ransomware\n" +
                        "• Social engineering\n" +
                        "• Two-factor authentication and OTP safety\n" +
                        "• Online privacy and app permissions\n" +
                        "• Public Wi-Fi and VPNs\n" +
                        "• Banking scams and SIM swap fraud\n" +
                        "• WhatsApp and social media account safety\n" +
                        "• Online shopping scams\n" +
                        "• Cyberbullying and reporting online abuse",

                        "I can help with many cybersecurity topics such as passwords, phishing, malware, privacy, social engineering, suspicious links, safe browsing, banking fraud, WhatsApp scams, public Wi-Fi, and 2FA."
                    },
                    "Help"
                ),

                // Password safety
                new Response(
                    new[]
                    {
                        "password", "passwords", "strong password", "secure password", "password safety",
                        "weak password", "stolen password", "leaked password", "password got leaked",
                        "someone knows my password", "change password", "password hacked", "login details",
                        "credentials", "passcode", "account password", "reuse password", "same password",
                        "birthday password", "name password"
                    },
                    new List<string>
                    {
                        "PASSWORD SAFETY TIPS:\n1. Use at least 12 characters.\n2. Mix uppercase letters, lowercase letters, numbers, and symbols.\n3. Do not reuse passwords across accounts.\n4. Avoid personal details like your name, surname, phone number, or birthday.\n5. Use a password manager where possible.",
                        "A strong password should be long, unique, and difficult to guess. Avoid names, dates of birth, phone numbers, and common words. A password manager can help you create and store strong passwords safely.",
                        "If you think your password has been stolen, change it immediately, enable two-factor authentication, and check whether the same password was reused on other accounts.",
                        "Never share your password through WhatsApp, SMS, email, or phone calls. A real company should not ask for your password."
                    },
                    "Password Safety"
                ),

                // Password manager explanation
                new Response(
                    new[] { "password manager", "password vault", "store passwords", "save passwords", "remember passwords", "password app" },
                    new List<string>
                    {
                        "A password manager stores your passwords securely in one place. It also helps generate strong and unique passwords so you do not need to reuse the same one everywhere.",
                        "Using a password manager is one of the best ways to improve password security. You only need to remember one strong master password.",
                        "A password manager is safer than writing passwords in a notebook or saving them in plain text on your phone."
                    },
                    "Password Safety"
                ),

                // Two-factor authentication and OTP safety
                new Response(
                    new[]
                    {
                        "2fa", "two factor authentication", "two-factor authentication", "multi factor authentication",
                        "multi-factor authentication", "mfa", "authentication", "otp", "one time pin",
                        "verification code", "login code", "sms code", "authenticator app", "google authenticator",
                        "microsoft authenticator", "security code", "approve login"
                    },
                    new List<string>
                    {
                        "Two-factor authentication adds an extra layer of security to your accounts. Even if someone steals your password, they still need a second verification step.",
                        "Never share your OTP or verification code with anyone. Criminals often pretend to be from a bank or delivery company to trick people into sharing OTPs.",
                        "An authenticator app is usually safer than SMS OTP because SIM swap fraud can allow criminals to receive SMS codes.",
                        "If you receive an OTP that you did not request, do not approve anything. Change your password and check your account activity."
                    },
                    "Authentication"
                ),

                // Phishing awareness
                new Response(
                    new[]
                    {
                        "phishing", "phishing scams", "phish", "scam", "scams", "scam email", "fake email",
                        "suspicious email", "email asking for password", "fake message", "urgent message",
                        "click this link", "verify your account", "account will be blocked", "bank email",
                        "fake bank", "fake capitec", "fake fnb", "fake standard bank", "fake absa",
                        "someone sent me a link", "i clicked a link", "dangerous email"
                    },
                    new List<string>
                    {
                        "Phishing is when criminals pretend to be trusted sources to steal your information. Be careful with urgent messages asking for passwords, OTPs, or banking details.",
                        "If a message pressures you to act quickly, it may be a phishing attempt. Pause, check the sender, and avoid clicking links directly from the message.",
                        "A safe habit is to open your bank or account website by typing the address yourself instead of clicking links from emails or SMS messages.",
                        "If you clicked a suspicious link, do not enter any details. Close the page, change important passwords, and monitor your accounts for unusual activity."
                    },
                    "Phishing"
                ),

                // Suspicious links
                new Response(
                    new[]
                    {
                        "link", "links", "suspicious link", "unsafe link", "unknown link", "short link",
                        "bitly", "tinyurl", "strange link", "weird link", "malicious link", "bad link",
                        "clicked link", "clicking links", "is this link safe"
                    },
                    new List<string>
                    {
                        "Always check links before clicking them. Look for spelling mistakes, strange domain names, or links that do not match the company they claim to represent.",
                        "If something feels suspicious, do not click the link. Rather visit the official website directly or contact the organisation through verified channels.",
                        "Shortened links can hide the real website address. Be extra careful when a shortened link comes from someone you do not know.",
                        "If you already clicked a suspicious link, avoid typing passwords or OTPs, close the page, and change your password if you entered anything."
                    },
                    "Suspicious Links"
                ),

                // Safe browsing
                new Response(
                    new[]
                    {
                        "safe browsing", "browsing", "internet safety", "online safety", "browse safely",
                        "website safety", "safe website", "unsafe website", "https", "padlock", "browser",
                        "pop up", "popup", "ads", "fake website", "dangerous website"
                    },
                    new List<string>
                    {
                        "Always check website safety before entering personal information. Look for HTTPS, correct spelling, and a trusted domain name.",
                        "Avoid clicking unknown links and keep your browser updated. Updates often fix security weaknesses that criminals can exploit.",
                        "Do not trust pop-ups that say your device is infected and demand urgent action. Many of those warnings are fake.",
                        "A safe browsing habit is to type important websites yourself, especially banking, school, email, and shopping websites."
                    },
                    "Safe Browsing"
                ),

                // Malware, viruses, spyware
                new Response(
                    new[]
                    {
                        "malware", "virus", "trojan", "worm", "spyware", "keylogger", "infected",
                        "my laptop is infected", "my phone is infected", "computer virus", "phone virus",
                        "device acting weird", "phone acting weird", "laptop acting weird", "slow phone",
                        "slow laptop", "unknown app", "strange app", "suspicious download", "downloaded file"
                    },
                    new List<string>
                    {
                        "Malware is harmful software that can damage your system, steal data, or spy on your activity. Avoid suspicious downloads and keep protection software updated.",
                        "If your device is acting strangely, check for unknown apps, run a security scan, update the system, and avoid logging into sensitive accounts until it is safe.",
                        "Spyware can secretly monitor activity, while keyloggers can record what you type. This is why downloading files from unknown websites is risky.",
                        "If you suspect malware, disconnect from the internet if necessary, scan the device, remove suspicious apps, and change passwords from a clean device."
                    },
                    "Malware"
                ),

                // Ransomware
                new Response(
                    new[]
                    {
                        "ransomware", "files encrypted", "locked files", "pay hackers", "ransom", "decrypt files",
                        "my files are locked", "cannot open files", "encrypted my files"
                    },
                    new List<string>
                    {
                        "Ransomware locks or encrypts your files and demands payment. The best protection is regular backups, updated software, and avoiding suspicious attachments.",
                        "If ransomware attacks a device, disconnect it from the network to stop it spreading. Do not rush to pay because payment does not guarantee recovery.",
                        "Keep backups on an external drive or secure cloud storage. A backup can save you if ransomware damages your files."
                    },
                    "Ransomware"
                ),

                // Social engineering
                new Response(
                    new[]
                    {
                        "social engineering", "manipulation", "pretexting", "baiting", "impersonation",
                        "pretending to be", "fake support", "fake technician", "someone called me",
                        "phone call scam", "they asked for details", "trust scam", "romance scam"
                    },
                    new List<string>
                    {
                        "Social engineering tricks people into giving away sensitive information by using fear, trust, pressure, or curiosity.",
                        "Always verify before sharing personal details. If someone claims to be from your bank, hang up and call the official number yourself.",
                        "Criminals may pretend to be IT support, bank staff, delivery companies, or even friends. The safest response is to verify independently.",
                        "Be careful when someone creates urgency, such as saying your account will be closed immediately. Pressure is a common scam tactic."
                    },
                    "Social Engineering"
                ),

                // Privacy
                new Response(
                    new[]
                    {
                        "privacy", "online privacy", "protect my privacy", "personal information", "personal details",
                        "data privacy", "share information", "oversharing", "who can see my posts",
                        "privacy settings", "location sharing", "tracking", "apps track me"
                    },
                    new List<string>
                    {
                        "Protect your privacy by reviewing account settings, limiting who can see your posts, and avoiding oversharing personal information online.",
                        "Personal information like your ID number, address, school, workplace, and location can be misused by criminals. Share less and check your privacy settings often.",
                        "Turn off location sharing for apps that do not need it. Many apps request more access than necessary.",
                        "Before posting online, ask yourself whether the information could help someone guess your passwords or impersonate you."
                    },
                    "Privacy"
                ),

                // App permissions and mobile safety
                new Response(
                    new[]
                    {
                        "app permissions", "permissions", "camera permission", "microphone permission", "location permission",
                        "phone security", "mobile security", "phone hacked", "android security", "iphone security",
                        "download app", "unknown app", "apk", "jailbreak", "rooted phone"
                    },
                    new List<string>
                    {
                        "Check app permissions regularly. A simple calculator app should not need access to your camera, microphone, contacts, or location.",
                        "Download apps only from trusted stores and avoid unknown APK files. Fake apps can steal data or install malware.",
                        "Keep your phone updated and use a screen lock. Mobile devices often store banking apps, email, photos, and personal information.",
                        "If you think your phone is hacked, remove suspicious apps, update the phone, change passwords, and check for unknown login sessions."
                    },
                    "Mobile Security"
                ),

                // Public Wi-Fi and VPN
                new Response(
                    new[]
                    {
                        "public wifi", "public wi-fi", "free wifi", "free wi-fi", "coffee shop wifi",
                        "airport wifi", "school wifi", "mall wifi", "unsafe wifi", "wifi safety",
                        "wi-fi safety", "vpn", "virtual private network", "use vpn"
                    },
                    new List<string>
                    {
                        "Public Wi-Fi can be risky because attackers may try to intercept information. Avoid logging into banking accounts on public Wi-Fi.",
                        "When using public Wi-Fi, avoid entering passwords or banking details. A trusted VPN can add protection by encrypting your connection.",
                        "Do not automatically connect to unknown Wi-Fi networks. Criminals can create fake hotspots with names that look legitimate.",
                        "A VPN helps protect your traffic on unsafe networks, but it does not make phishing links or fake websites safe. You still need to be careful."
                    },
                    "Public WiFi"
                ),

                // Banking fraud and South African context
                new Response(
                    new[]
                    {
                        "bank scam", "banking scam", "bank fraud", "banking fraud", "online banking",
                        "bank account hacked", "money stolen", "stolen money", "capitec scam", "fnb scam",
                        "absa scam", "standard bank scam", "nedbank scam", "fake banking app",
                        "banking details", "card details", "credit card", "debit card", "eft scam",
                        "payment scam", "refund scam"
                    },
                    new List<string>
                    {
                        "Banking scams are common in South Africa. Never share your banking password, card PIN, CVV, or OTP with anyone, even if they claim to work for your bank.",
                        "If someone says there is a problem with your bank account, do not use the number or link they sent. Contact your bank using the official banking app or official number.",
                        "Fake banking apps and fake bank websites can steal login details. Download banking apps only from official app stores and verify the developer name.",
                        "If money was stolen from your account, contact your bank immediately, change your password, disable compromised cards, and report the incident."
                    },
                    "Banking Security"
                ),

                // SIM swap fraud
                new Response(
                    new[]
                    {
                        "sim swap", "sim swap fraud", "sim card hacked", "lost signal", "no network",
                        "phone number stolen", "sim replacement", "sim fraud", "otp stolen by sim"
                    },
                    new List<string>
                    {
                        "SIM swap fraud happens when criminals take control of your phone number to receive OTPs. If your phone suddenly loses network, contact your mobile provider immediately.",
                        "Protect yourself from SIM swap fraud by using app-based authentication where possible and setting strong security questions with your mobile provider.",
                        "If you suspect a SIM swap, call your network provider, contact your bank, and secure your email accounts immediately."
                    },
                    "SIM Swap Fraud"
                ),

                // WhatsApp scams
                new Response(
                    new[]
                    {
                        "whatsapp", "whatsapp scam", "whatsapp hacked", "whatsapp code", "whatsapp verification",
                        "verification code scam", "friend asking for money", "whatsapp account stolen",
                        "someone took my whatsapp", "whatsapp otp"
                    },
                    new List<string>
                    {
                        "WhatsApp scams often involve criminals asking for your verification code. Never share your WhatsApp code with anyone.",
                        "If a friend suddenly asks for money on WhatsApp, call them first to confirm. Their account may have been hacked.",
                        "Enable two-step verification on WhatsApp. This adds a PIN that helps protect your account from takeover.",
                        "If your WhatsApp is stolen, try to re-register your number, enable two-step verification, and warn your contacts not to send money."
                    },
                    "WhatsApp Security"
                ),

                // Social media security
                new Response(
                    new[]
                    {
                        "facebook hacked", "instagram hacked", "tiktok hacked", "social media hacked",
                        "account hacked", "hacked account", "profile hacked", "someone logged in",
                        "unknown login", "recover account", "facebook scam", "instagram scam"
                    },
                    new List<string>
                    {
                        "If a social media account is hacked, change the password, remove unknown devices, enable two-factor authentication, and warn friends not to trust suspicious messages from your account.",
                        "Check login activity on your social media accounts. If you see unknown locations or devices, sign them out and change your password.",
                        "Avoid clicking giveaway links or messages promising prizes. Many social media scams are designed to steal login details.",
                        "Use different passwords for email and social media. If one account is hacked, criminals should not be able to access everything."
                    },
                    "Social Media Security"
                ),

                // Identity theft
                new Response(
                    new[]
                    {
                        "identity theft", "stolen identity", "identity", "id number", "id document",
                        "someone used my name", "impersonation", "personal information stolen",
                        "my details were stolen", "data stolen"
                    },
                    new List<string>
                    {
                        "Identity theft happens when criminals use your personal information to pretend to be you. Protect your ID number, address, banking details, and account passwords.",
                        "If your personal details were stolen, monitor your accounts, change passwords, contact your bank if necessary, and report suspicious activity.",
                        "Do not upload ID documents to unknown websites. Criminals can use those documents for fraud or impersonation."
                    },
                    "Identity Theft"
                ),

                // Data breaches
                new Response(
                    new[]
                    {
                        "data breach", "breach", "leaked data", "data leak", "information leaked",
                        "email leaked", "password leaked", "company hacked", "my data was leaked"
                    },
                    new List<string>
                    {
                        "A data breach happens when private information is exposed without permission. If your details are leaked, change affected passwords and watch for phishing messages.",
                        "After a breach, criminals may use leaked information to create convincing scams. Be extra careful with emails and SMS messages that mention your real details.",
                        "If your password was leaked, change it everywhere you reused it. This is why unique passwords are important."
                    },
                    "Data Breach"
                ),

                // Online shopping scams
                new Response(
                    new[]
                    {
                        "online shopping", "shopping scam", "fake shop", "fake store", "facebook marketplace",
                        "takealot scam", "gumtree scam", "marketplace scam", "too cheap", "seller scam",
                        "delivery scam", "courier scam", "payment before delivery"
                    },
                    new List<string>
                    {
                        "Online shopping scams often use prices that look too good to be true. Check reviews, website age, payment options, and contact details before buying.",
                        "Avoid paying strangers upfront without protection. Use trusted platforms and be careful with sellers who pressure you to pay quickly.",
                        "Fake delivery messages may ask for a small fee or card details. Verify deliveries through the official courier website or app.",
                        "Before buying from an online store, check the website spelling, return policy, secure payment options, and whether the business has real contact details."
                    },
                    "Online Shopping Safety"
                ),

                // Email attachments
                new Response(
                    new[]
                    {
                        "attachment", "email attachment", "pdf attachment", "invoice attachment",
                        "open attachment", "word document", "excel file", "macro", "unknown attachment",
                        "download attachment"
                    },
                    new List<string>
                    {
                        "Be careful with email attachments, especially invoices, PDFs, Word documents, and Excel files from unknown senders. They can carry malware.",
                        "Do not enable macros in documents unless you fully trust the source. Malicious macros can install malware.",
                        "If an attachment was unexpected, verify with the sender using a separate communication method before opening it."
                    },
                    "Email Security"
                ),

                // USB safety
                new Response(
                    new[]
                    {
                        "usb", "flash drive", "memory stick", "external drive", "unknown usb",
                        "found usb", "infected usb", "plug usb"
                    },
                    new List<string>
                    {
                        "Do not plug unknown USB drives into your computer. They can contain malware or tools that compromise your device.",
                        "Scan USB drives before opening files, and avoid using USB devices from unknown people or public places.",
                        "A found flash drive may look harmless, but it can be used as bait in a cyberattack."
                    },
                    "USB Safety"
                ),

                // Cloud security
                new Response(
                    new[]
                    {
                        "cloud", "google drive", "onedrive", "icloud", "dropbox", "cloud storage",
                        "share file", "shared link", "backup online", "cloud backup"
                    },
                    new List<string>
                    {
                        "Protect cloud accounts with strong passwords and two-factor authentication because cloud storage often contains private documents and photos.",
                        "Be careful when sharing cloud links. Anyone with the link may access the file if permissions are too open.",
                        "Regular cloud backups can protect you from losing data, but make sure your account itself is secured."
                    },
                    "Cloud Security"
                ),

                // Updates and patching
                new Response(
                    new[]
                    {
                        "update", "updates", "software update", "system update", "windows update",
                        "patch", "patching", "security update", "outdated software", "update phone"
                    },
                    new List<string>
                    {
                        "Software updates are important because they fix security weaknesses. Delaying updates can leave your device exposed.",
                        "Enable automatic updates where possible for your operating system, browser, antivirus, and important apps.",
                        "Updates may seem annoying, but many cyberattacks target old security flaws that have already been fixed."
                    },
                    "Software Updates"
                ),

                // Antivirus
                new Response(
                    new[]
                    {
                        "antivirus", "anti virus", "windows defender", "security scan", "scan device",
                        "malware protection", "virus protection", "real time protection"
                    },
                    new List<string>
                    {
                        "Antivirus software helps detect and block malware. Keep it updated and run scans if your device behaves strangely.",
                        "Windows Defender is useful when kept updated, but safe behaviour is still important. Antivirus cannot protect you from every scam.",
                        "Use antivirus together with safe browsing, strong passwords, updates, and careful downloading habits."
                    },
                    "Antivirus"
                ),

                // Cyberbullying and online abuse
                new Response(
                    new[]
                    {
                        "cyberbullying", "online bullying", "harassment", "online abuse", "threatening messages",
                        "someone is bullying me", "mean messages", "report abuse", "block someone"
                    },
                    new List<string>
                    {
                        "Cyberbullying is online harassment or abuse. Do not respond aggressively. Save evidence, block the person, report the account, and speak to someone you trust.",
                        "If someone threatens you online, take screenshots, report the account, and contact a trusted adult, institution, or local authority if you feel unsafe.",
                        "Most platforms allow you to block and report abusive users. Your safety and peace of mind matter."
                    },
                    "Cyberbullying"
                ),

                // AI and deepfake scams
                new Response(
                    new[]
                    {
                        "ai scam", "deepfake", "fake voice", "fake video", "voice cloning", "ai generated",
                        "fake celebrity", "investment video", "deepfake scam"
                    },
                    new List<string>
                    {
                        "AI scams can use fake voices or videos to impersonate trusted people. Verify requests through another channel before sending money or information.",
                        "Deepfake videos can look convincing. Be careful with investment adverts, celebrity endorsements, and urgent requests that seem unusual.",
                        "If someone sends a voice note asking for money, call them directly on a trusted number before believing it."
                    },
                    "AI Scams"
                ),

                // Cryptocurrency and investment scams
                new Response(
                    new[]
                    {
                        "crypto scam", "bitcoin scam", "investment scam", "forex scam", "trading scam",
                        "guaranteed returns", "double your money", "money making scheme", "ponzi", "pyramid scheme"
                    },
                    new List<string>
                    {
                        "Be careful with investments promising guaranteed returns or quick profits. Scammers often use pressure and fake success stories.",
                        "Never send money to someone who promises to double it quickly. Real investments carry risk and should be checked carefully.",
                        "Crypto and forex scams often use fake screenshots, fake testimonials, and urgent deadlines. Verify before investing."
                    },
                    "Investment Scams"
                ),

                // Children and family online safety
                new Response(
                    new[]
                    {
                        "children online", "kids online", "child safety", "parental control", "family safety",
                        "protect children", "online predators", "screen time"
                    },
                    new List<string>
                    {
                        "Children should be taught not to share personal details, school information, location, or photos with strangers online.",
                        "Parents can use privacy settings, parental controls, and open conversations to help children stay safe online.",
                        "A good rule is that children should tell a trusted adult if someone online makes them uncomfortable or asks for private information."
                    },
                    "Family Online Safety"
                ),

                // Gaming security
                new Response(
                    new[]
                    {
                        "gaming", "game account", "gaming account", "roblox", "fortnite", "steam",
                        "free skins", "free coins", "game scam", "hacked game account"
                    },
                    new List<string>
                    {
                        "Gaming scams often promise free skins, coins, or upgrades. These links may steal your login details.",
                        "Protect gaming accounts with strong passwords and two-factor authentication, especially if purchases are linked to the account.",
                        "Do not share game account passwords or verification codes. Scammers may pretend to be friends or game moderators."
                    },
                    "Gaming Security"
                ),

                // Smart devices and IoT
                new Response(
                    new[]
                    {
                        "smart device", "iot", "smart tv", "smart camera", "wifi camera", "router",
                        "home wifi", "smart home", "default password", "camera hacked"
                    },
                    new List<string>
                    {
                        "Smart devices should not keep default passwords. Change default login details and update device firmware.",
                        "Secure your home Wi-Fi with a strong password and WPA2 or WPA3 encryption if available.",
                        "Smart cameras and routers can be targeted if left with default passwords. Change them immediately after setup."
                    },
                    "Smart Device Security"
                ),

                // Hacked or compromised accounts - broad human wording
                new Response(
                    new[]
                    {
                        "hacked", "i was hacked", "someone hacked me", "compromised", "breached",
                        "someone accessed my account", "locked out", "account taken", "account stolen",
                        "cannot login", "strange activity", "unusual activity", "unknown device"
                    },
                    new List<string>
                    {
                        "If you think an account was hacked, change the password immediately, enable two-factor authentication, remove unknown devices, and check recovery email or phone number settings.",
                        "Start with your email account because it is often used to reset other passwords. Secure email first, then secure banking and social media accounts.",
                        "Check account activity for unknown logins. Sign out of all sessions and report the compromise to the service provider if needed.",
                        "If you are locked out, use the official account recovery page. Do not trust people online who claim they can recover accounts for a fee."
                    },
                    "Hacked Account"
                ),

                // South African cyber threats general
                new Response(
                    new[]
                    {
                        "south africa", "sa", "south african", "cybercrime act", "cybercrime in south africa",
                        "local scams", "saps", "report cybercrime", "south african scams"
                    },
                    new List<string>
                    {
                        "In South Africa, common threats include phishing, SIM swap fraud, fake banking messages, WhatsApp scams, and online shopping scams. Never share OTPs or banking details.",
                        "If you experience cybercrime, collect evidence such as screenshots, messages, numbers, emails, and transaction details before reporting it.",
                        "South African users should be especially careful with banking OTPs, SIM swap signs, fake delivery messages, and impersonation scams."
                    },
                    "South Africa"
                ),

                // Exit messages
                new Response(
                    new[] { "bye", "goodbye", "exit", "quit", "thank you", "thanks", "see you", "done" },
                    new List<string>
                    {
                        "Thank you for using the Cybersecurity Awareness Bot. Stay safe online, {userName}!",
                        "Goodbye, {userName}. Remember to stay safe online.",
                        "You are welcome, {userName}. Keep practising safe online habits."
                    },
                    "Exit"
                )
            };
        }
    }
}
