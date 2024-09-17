using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.F4A.MobileThird;

namespace DataCore
{
    public class ConfigManager : SingletonMonoAwake<ConfigManager>
    {
        #region define flag        

        public bool IsLog = false;

        public bool IsLogFormat = false;

        public bool IsLogInfor = false;

        public bool IsLogWarning = false;

        public bool IsLogError = false;

        public bool IsLogException = false;
        #endregion

        #region define variable
        // version file config current in game
        public static int TimeOutConfig = 15;

        // version file config current in game
        public static int TimeShowIntConfig = 2;

        // version file config current in game
        public static int TimeShowPopupHint = 120;

        // version file config current in game
        public static string TimeLoadAds2Day = "TimeLoadAds2Day";

        // version file config current in game
        public static int TimeShowIntArtBlitzConfig = 2;

        // version file config current in game
        public static string TimeShowintArtBlitz = "TimeShowintArtBlitz";

        // version file config current in game
        public static string BannerType = "AdaptiveBanner";

        // version file config current in game
        public static string VersionConfig = "VersionConfig";

        // ID user after login : facebook , google ,apple
        public static string IdUser = "IdUser";
        public static string LoginType = "LoginType";

        // Name file data of user will store de byte data from the object.
        public static string FileNameProfile = "UserProfile";


        // Name file data of game will store de byte data from the object.
        public static string FileNameGameData = "GameData";

        // Name file data of game will store de byte data from the object.
        public static string FileNamePuzzleData = "PuzzleData";
        // You client id of google sing in
        public static string WebClientId = "97829762991-qbkej8efsif41fgjms6n7f2c5pp46alg.apps.googleusercontent.com";
        // version file config current in game
        public static int Continuously3Puzzles = 3;

        // version file config current in game
        public static string UserPlaysContinuously3Puzzles = "UserPlaysContinuously3Puzzles";

        // Name file data of game will store de byte data from the object.
        public static string EnableDebugMode = "EnableDebugMode";

        // Coin gift login success.
        public static int CoinGiftLoginSuccess = 500;

        // Key save step game.
        public static string StepGame = "StepGame";

        // Key save loginSuccess.
        public static string LoginSuccess = "LoginSuccess";

        // Key save loginSuccess.
        public static string LoginFirstSuccess = "LoginFirstSuccess";

        // Key save push notification permission.
        public static string TokenSent = "tokenSent";

        // description daily reward gift after buy subscription.
        public static string MsgShowLoginFail = "Login not success";

        #endregion
        // Url more info iap.
        public static string UrlTermsInfo = "https://artstory.helpdocsonline.com/terms";

        // Url more info iap.
        public static string UrlPolicyInfo = "https://artstory.helpdocsonline.com/privacy";

        #region Iap
        // Url more info iap.
        public static string UrlMoreInfo = "https://artstory.helpdocsonline.com/premium";

        // description daily reward gift after buy subscription.
        public static string DescriptionDailyGift = "Thank you for supporting ArtStory Next reward in";

        // Coin daily gift subscription.
        public static int InksDailyRewardGift = 500;

        // key buy subscription success.
        public static string KeyBuySubscriptionSuccess = "KeyBuySubscriptionSuccess";

        // key time begin subscription success.
        public static string KeyTimeBeginSubscription = "KeyTimeBeginSubscription";

        public static string KeyTimeExpiredSubscription = "KeyTimeExpiredSubscription";

        // key get gift daily subscription done .
        public static string DailySubscriptionDone = "DailySubscriptionDone";

        public const string LastTimeClaimDailySubscriptionReward = "last_time_claim_daily_subscription_reward";
        #endregion

        #region localization
        public static List<int> IdLanguege = new List<int>() { 0 };
        #endregion


        #region local push notification
        public static string LastTimeBreakHighScore = "LastTimeBreakHighScore";

        public static string TotalDuration = "TotalDuration";
        #endregion

        #region message loading
        public static string YouCanTurnOffMusicInSetting = "You can turn off music in Setting.";
        public static string YouCanTurnOffSoundInSetting = "You can turn off sound in Setting.";
        public static string YouCanTurnOffHapticInSetting = "You can turn off haptic in Setting.";
        public static string YouCanTurnOffDescriptionInSetting = "You can turn off description in Setting.";
        public static string ShareyourFeedbackWithUsViaSupportInSetting = "Share your feedback with us via Support in Setting.";
        public static string TurnOnPushNotificationToReceiveLatestUpdates = "Turn on push notification to receive latest updates.";
        public static string LogInToPlayOnMultipleDevicesAndSaveProgress = "Log in to play on multiple devices and save progress.";
        public static string HopOnArtStoryEverydayToEarnDailyGifts = "Hop on Art Story everyday to earn daily gifts.";
        public static string EarnRewardsViaLuckySpin = "Earn rewards via Lucky Spin.";
        public static string KeepYouInternetStableWhileDownloadingBooks = "Keep you internet stable while downloading books.";
        #endregion

        #region tracking
        public const string LastTimeSendSessionEventToTenjin = "last_time_send_session_event_to_tenjin";
        public const string LastTimeSendAttributionEventToTenjin = "last_time_send_attribution_event_to_tenjin";

        #endregion


        public class GameData
        {
            public static string adjustId = "adjust_id";
            public static string install_version = "install_version";
            public static string install_date = "install_date";

            public class PlayType
            {
                public static string new_puzzle = "new";
                public static string resume_puzzle = "resume";
                public static string replay_puzzle = "replay";
                public static string free_puzzle = "free_to_play";
                public static string collection_play_puzzle = "collection_play_puzzle";
            }
            public class ChapterOpenTriggerSource
            {
                public static string top_picks = "top_picks";
                public static string book_details = "book_details";
            }
            public class BookOpenTriggerSource
            {
                public static string top_picks = "featured_book";
                public static string book_details = "library";
                public static string all_collections = "all_collections";
            }
            public class ResourceType
            {
                public static string ink = "ink_drop";
                public static string hint = "hint";
            }
            public class ResourceEarnSource
            {
                public static string buy_iap = "buy_iap";
                public static string puzzle_reward = "puzzle_reward";
                public static string lucky_draw = "lucky_draw";
                public static string puzzle_hint = "puzzle_hint";
                public static string daily_reward = "daily_reward";
                public static string daily_subscription_reward = "daily_subscription_reward";
                public static string daily_challenge = "daily_challenge";
                public static string login_reward = "login_reward";
                public static string welcome_gift = "welcome_gift";
                public static string pre_registration = "pre_registration";
                public static string redoom_reward = "redoom_reward";
                public static string live_event = "live_event";
            }

            public class ResourceSpentSource
            {
                public static string unlock_chapter = "unlock_chapter";
                public static string hint_puzzle = "hint_puzzle";
            }

        }
        public class TrackingEvent
        {
            public class EventName
            {
                public static string user_subscription = "user_subscription";
                public static string purchase_iap = "purchase_iap";
                public static string showed_interstitial_ad = "showed_interstitial_ad";
                public static string clicked_interstitial_ad = "clicked_interstitial_ad";
                public static string closed_interstitial_ad = "closed_interstitial_ad";
                public static string failed_to_show_interstitial_ad = "failed_to_show_interstitial_ad";
                public static string showed_rewarded_ad = "showed_rewarded_ad";
                public static string clicked_rewarded_ad = "clicked_rewarded_ad";
                public static string closed_rewarded_ad = "closed_rewarded_ad";
                public static string completed_rewarded_ad = "completed_rewarded_ad";
                public static string failed_to_show_rewarded_ad = "failed_to_show_rewarded_ad";
                public static string resource_earn = "resource_earn";
                public static string resource_spent = "resource_spent";
                public static string start_puzzle = "start_puzzle";
                public const string play_puzzle = "play_puzzle";
                public const string failed_to_load_puzzle = "failed_to_load_puzzle";
                public static string completed_puzzle = "completed_puzzle";
                public static string shared_puzzle = "shared_puzzle";
                public static string downloaded_puzzle = "downloaded_puzzle";
                public static string unlock_chapter = "unlock_chapter";
                public static string open_chapter = "open_chapter";
                public static string open_book = "open_book";
                public static string claim_daily_challenge = "claim_daily_challenge";
                public static string tuto_completed = "tuto_completed";
                public static string open_unlock_first_chapter = "open_unlock_first_chapter";
                public static string unlock_first_chapter = "unlock_first_chapter";
                public static string receive_welcome_gift = "receive_welcome_gift";
                public static string played_three_puzzle = "played_three_puzzle";
                public static string played_five_puzzle = "played_five_puzzle";
                public static string played_seven_puzzle = "played_seven_puzzle";
                public static string completed_three_rewarded_ad = "completed_three_rewarded_ad";
                public static string completed_five_rewarded_ad = "completed_five_rewarded_ad";
                public static string completed_seven_rewarded_ad = "completed_seven_rewarded_ad";
                public static string shown_three_interstitial_ad = "shown_three_interstitial_ad";
                public static string shown_five_interstitial_ad = "shown_five_interstitial_ad";
                public static string shown_seven_interstitial_ad = "shown_seven_interstitial_ad";
                public const string PurchaseNotVerifiedVerification = "PurchaseNotVerifiedVerification";
                public const string PurchaseUnknownVerification = "PurchaseUnknownVerification";
                public const string PurchaseFailedVerification = "PurchaseFailedVerification";
                public const string downloaded_asset_bundle = "downloaded_asset_bundle";
                public const string downloaded_puzzle_asset_bundle = "downloaded_puzzle_asset_bundle";
                public const string session_ad_revenue = "yomi_ad_revenue_in_session";
                public const string ad_revenue = "yomi_ad_revenue";
                public const string mobile_attribution = "mobile_attribution";
                public const string start_tutorial_puzzle = "start_tutorial_puzzle";
                public const string play_tutorial_puzzle = "play_tutorial_puzzle";
                public const string completed_tutorial_puzzle = "completed_tutorial_puzzle";
                public const string completed_step_tutorial_puzzle = "tut_step";
                public const string first_open_main_game = "first_open_main_game";
                public const string show_puzzle_description_1 = "show_puzzle_description_1";
                public const string completed_puzzle_description_1 = "completed_puzzle_description_1";
                public static string unlock_second_chapter = "unlock_second_chapter";
                public static string open_unlock_new_book = "open_unlock_new_book";
                public static string completed_unlock_new_book = "completed_unlock_new_book";
                public static string shown_ad_notice = "shown_ad_notice";
                public static string shown_1st_interstitial_ad = "shown_1st_interstitial_ad";
                public static string conversion_value = "conversion_value";
            }

            public class EventParam
            {
                public static string transaction_date = "transaction_date";
                public static string transaction_id = "transaction_id";
                public static string product_id = "product_id";
                public static string currency = "currency";
                public static string price = "price";
                public static string purchase_time = "purchase_time";
                public static string purchase_token = "purchase_token";
                public static string order_id = "order_id";
                public static string ad_type = "ad_type";
                public static string country_code = "country_code";
                public static string ad_unit_identifier = "ad_unit_identifier";
                public static string network_name = "network_name";
                public static string network_placement = "network_placement";
                public static string placement = "placement";
                public static string creative_identifier = "creative_identifier";
                public static string revenue = "revenue";
                public static string impression = "impression";
                public static string trigger_source = "trigger_source";
                public static string resource_type = "resource_type";
                public static string amount = "amount";
                public static string play_type = "play_type";
                public static string puzzle_name = "puzzle_name";
                public static string used_hints = "used_hints";
                public static string played_time = "played_time";
                public static string chapter_id = "chapter_id";
                public static string chapter_name = "chapter_name";
                public static string book_name = "book_name";
                public static string book_id = "book_id";
                public static string challenge_id = "challenge_id";
                public static string bundle_name = "bundle_name";
                public static string bundle_size = "bundle_size";
                public static string download_duration = "download_duration";
                public static string conversion_value = "conversion_value";
            }
        }
        #region ads
        public static int NumberIncreaseHint = 1;
        public static int NumberIncreasePopupHint = 2;
        public static int keyMaxPuzzleUpdateShowAds = 7;
#if UNITY_IOS || UNITY_EDITOR
        public static string keyMaxPuzzlePlayShowIntertitialAds = "3";

        public static string TimePlayPuzzleShowIntertitialAds = "ios_time_play_puzzle_show_interstitial_ads";
        // Time delay show hand tutorial
        public static string KeyTimeDelayShowHandTutorial = "ios_time_delay_show_hand_tutorial";

#elif UNITY_ANDROID
        public static string TimePlayPuzzleShowIntertitialAds = "android_time_play_puzzle_show_interstitial_ads";
        // Time delay show hand tutorial
        public static string KeyTimeDelayShowHandTutorial = "android_time_delay_show_hand_tutorial";
        public static string keyMaxPuzzlePlayShowIntertitialAds = "3";
       

#endif

        public static int MaxShowAdsLuckyInDay = 5;
        public static string KeyMaxShowAdsLuckyInDay = "KeyMaxShowAdsLuckyInDay";

        public static int TimeForeground = 45;
        public static string KeyTimeForeground = "KeyTimeForeground";

        public static string keyIosBackgroundAd = "ios_background_ad";
        public static string keyAndroidBackgroundAd = "android_background_ad";

        public static string keyIosLoadingAd = "ios_loading_ad";
        public static string keyAndroidLoadingAd = "android_loading_ad";

        public static string keyIosDoubleLuckyAd = "ios_double_lucky_ad";
        public static string keyAndroidDoubleLuckyAd = "android_double_lucky_ad";

        public static string keyIosDoubleDailyAd = "ios_double_daily_ad";
        public static string keyAndroidDoubleDailyAd = "android_double_daily_ad";

        public static string keyIosBetweenGamesAd = "ios_between_games_ad";
        public static string keyAndroidBetweenGamesAd = "android_between_games_ad";
        #endregion

        #region PreRegistration
#if UNITY_IOS || UNITY_EDITOR

        // Enable pre registration
        public static string EnablePreRegistrationGift = "ios_enable_pre_registration_gift";
        // pre registration time end.
        public static string TimeEndPreRegistrationGift = "ios_time_end_pre_registration_gift";
        // Coin daily gift pre registration.
        public static string InkPreRegistrationGift = "ios_ink_pre_registration_gift";

        // pre registration time begin.
        public static string InkPreRegistrationGiftEndTime = "ios_ink_pre_registration_gift_end_time";

        // pre registration time begin.
        public static string TimeBeginPreRegistrationGift = "ios_time_begin_pre_registration_gift";
#elif UNITY_ANDROID
        public static string EnablePreRegistrationGift = "android_enable_pre_registration_gift";
        public static string TimeEndPreRegistrationGift = "android_time_end_pre_registration_gift";
        // Coin daily gift pre registration.
        public static string InkPreRegistrationGift = "android_ink_pre_registration_gift";

        // pre registration time begin.
        public static string InkPreRegistrationGiftEndTime = "android_ink_pre_registration_gift_end_time";

        // pre registration time begin.
        public static string TimeBeginPreRegistrationGift = "android_time_begin_pre_registration_gift";
#endif


        #endregion

        // key buy iap vip  .
        public static string keyVipIap = "keyVipIap";

        // key buy iap vip.
        public static string keyVipIapAds = "keyVipIapAds";

        // key buy iap vip  .
        public static string keyVipIapV2 = "keyVipIapV2";

        // key buy iap vip.
        public static string keyVipIapAdsV2 = "keyVipIapAdsV2";


        // key buy iap vip Unlimited hint .
        public static int keyVipNumberUntimitedHint = 99;

        //key redeem code
        public static string removeads = "removeads";

        //key redeem code
        public static string GET_DEVICE_ID = "GET_DEVICE_ID";

        //key redeem code
        public static string ENABLE_DEBUG_MODE = "ENABLE_DEBUG_MODE";

        //key redoom code msg success
        public static string MSG_REDOOM_CODE_SUCCESS = "Redeem Succesfully";

        //key redoom code msg error
        public static string MSG_REDOOM_CODE_ERROR = "Invalid redeem code, please try again";

        //key redoom code
        public static string KeyIncredibleIap = "KeyIncredibleIap";

        // version file config duration day daily reward
        public static int DayDailyReward = 2;

        // version file config time daily reward claim
        public static string TimeDailyRewardClaim = "TimeDailyRewardClaim";

        // Time delay show hand tutorial
        public static float TimeDelayShowHandTutorial = 5.0f;


        // total play puzzle finish
        public static string KeyTotalPlayPuzzleFinish = "KeyTotalPlayPuzzleFinish";

        // key time when open subscription auto
        public static string KeyTimeOpenSubscription = "KeyTimeOpenSubscription";

        // Number show tutorial
        public static int TIME_COUNT_SHOW_TUTORIAL = 2;

        //key restore code msg fail
        public static string MSG_RESTORE_CODE_FAILED = "There is no progress updated, please try again later.";

        //key restore code msg success
        public static string MSG_RESTORE_CODE_SUCCESS = "Restore progress successfully.";

#if UNITY_IOS || UNITY_EDITOR

        // get extra reward when complete puzzle
        public static string KeyGetExtraReward = "ios_get_extra_reward";

          // get extra reward when complete puzzle
        public static string KeyGetExtraRewardArtBlitz = "ios_get_extra_reward_Art_Blitz";

          // pre registration time begin.
        public static float GetExtraRewardValue = 2.0f;

        // pre registration time begin.
        public static float GetExtraRewardArtBlitzValue = 0.3f;

        // get extra reward when complete puzzle
        public static string KeyGetExtraRewardTutorial = "ios_get_extra_reward_tutorial";
#elif UNITY_ANDROID

        // get extra reward when complete puzzle
        public static string KeyGetExtraReward = "android_get_extra_reward";

        // get extra reward when complete puzzle
        public static string KeyGetExtraRewardArtBlitz = "android_get_extra_reward_Art_Blitz";

        // pre registration time begin.
        public static float GetExtraRewardValue = 2.0f;

        // pre registration time begin.
        public static float GetExtraRewardArtBlitzValue = 0.3f;

        // get extra reward when complete puzzle
        public static string KeyGetExtraRewardTutorial = "android_get_extra_reward_tutorial";
#endif

        //key Check Day free puzzle
        public static string KEY_FREE_PUZZLE_TODAY = "FreePuzzleToday";

        public static int Size_Cache_puzzle = 40;

        public static string TimeSyncWithCloud = "TimeSyncWithCloud";

        // key show live event tutorial.
        public static string KeyShowLiveEventTutorial = "KeyShowLiveEventTutorial";

        // key reward live event  .
        public static int KeyRewardLiveEventInk = 3000;

        // key reward live event  .
        public static int KeyRewardLiveEventHint = 5;

    }
}