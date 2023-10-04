<?php


function getWeaponUpgradeMultiplier($item_name) {
    $upgradeMultipliers = array(
        0 => 1,
        1 => 2,
        2 => 3,
        3 => 4,
        4 => 5,
        5 => 10
    );

    // Replace 'M416' with 'X' to handle the M416 case
    $item_name = str_replace("M416", "X", $item_name);

    preg_match('/(\d+)$/', $item_name, $matches);
    $upgradeLevel = isset($matches[0]) ? (int) $matches[0] : 0;

    return isset($upgradeMultipliers[$upgradeLevel]) ? $upgradeMultipliers[$upgradeLevel] : 1;
}


function getItemType($item_name) {
    $weapons = array("BLASTER", "QUADROCKET", "GRAVITYGUN", "REDLASER", "BLUELASER", "BUZZSAW", "DOUBLEBALL", "SINGLEBALL", "LASERLANCE", "LIGHTNINGER", "M416", "PLASMAGUN", "RAILER", "ROCKETER", "SHOTGUN", "SMG", "SNIPER", "TRIPLESHOT", "SLEDGER");

    foreach ($weapons as $weapon) {
        if (strpos($item_name, $weapon) !== false) {
            return "weapon";
        }
    }

    return "mod";
}



function getItemRank($item_name) {
    // Rank values for all weapons (you can adjust these values)

    $item_name = str_replace("M416", "X", $item_name);

    // Remove trailing numbers from the item name
    $item_name_without_numbers = preg_replace('/\d+$/', '', $item_name);



    $weaponRanks = array(
        "QUADROCKET" => 19,
        "ROCKETER" => 18,
        "TRIPLESHOT" => 17,
        "DOUBLEBALL" => 16,
        "SHOTGUN" => 15,
        "PLASMAGUN" => 14,
        "LASERLANCE" => 13,
        "SINGLEBALL" => 12,
        "RAILER" => 11,
        "M416" => 10,
        "REDLASER" => 9,
        "BLUELASER" => 8,
        "SMG" => 7,
        "GRAVITYGUN" => 6,
        "BUZZSAW" => 5,
        "LIGHTNINGER" => 4,
        "SNIPER" => 3,
        "SLEDGER" => 2,
        "BLASTER" => 1
    );
    
    $rankzeroweight = 1;
    $rankoneweight = 2;
    $ranktwoweight = 3;
    $rankthreeweight = 5;
    $rankfourweight = 10;
    $rankfiveweight = 15;

    
    $modRanks = array(
        "DAMAGEMOD" => $rankzeroweight, "DAMAGEMOD1" => $rankoneweight, "DAMAGEMOD2" => $ranktwoweight, "DAMAGEMOD3" => $rankthreeweight, "DAMAGEMOD4" => $rankfourweight, "DAMAGEMOD5" =>  $rankfiveweight,
        "BLASTMOD" => $rankzeroweight, "BLASTMOD1" => $rankoneweight, "BLASTMOD2" => $ranktwoweight, "BLASTMOD3" => $rankthreeweight, "BLASTMOD4" => $rankfourweight, "BLASTMOD5" => $rankfiveweight,
        "JUMPMOD" => $rankzeroweight, "JUMPMOD1" => $rankoneweight, "JUMPMOD2" => $ranktwoweight, "JUMPMOD3" => $rankthreeweight, "JUMPMOD4" => $rankfourweight, "JUMPMOD5" => $rankfiveweight,
        "MAXHPMOD" => $rankzeroweight, "MAXHPMOD1" => $rankoneweight, "MAXHPMOD2" => $ranktwoweight, "MAXHPMOD3" => $rankthreeweight, "MAXHPMOD4" => $rankfourweight, "MAXHPMOD5" => $rankfiveweight,
        "SPEEDMOD" => $rankzeroweight, "SPEEDMOD1" => $rankoneweight, "SPEEDMOD2" => $ranktwoweight, "SPEEDMOD3" => $rankthreeweight, "SPEEDMOD4" => $rankfourweight, "SPEEDMOD5" => $rankfiveweight,
        "REGENMOD" => $rankzeroweight, "REGENMOD1" => $rankoneweight, "REGENMOD2" => $ranktwoweight, "REGENMOD3" => $rankthreeweight, "REGENMOD4" => $rankfourweight, "REGENMOD5" => $rankfiveweight,
        "SIPHONMOD" => $rankzeroweight, "SIPHONMOD1" => $rankoneweight, "SIPHONMOD2" => $ranktwoweight, "SIPHONMOD3" => $rankthreeweight, "SIPHONMOD4" => $rankfourweight, "SIPHONMOD5" => $rankfiveweight,
        "LEECHMOD" => $rankzeroweight, "LEECHMOD1" => $rankoneweight, "LEECHMOD2" => $ranktwoweight, "LEECHMOD3" => $rankthreeweight, "LEECHMOD4" => $rankfourweight, "LEECHMOD5" => $rankfiveweight,
        "SHIELDMOD" => $rankzeroweight, "SHIELDMOD1" => $rankoneweight, "SHIELDMOD2" => $ranktwoweight, "SHIELDMOD3" => $rankthreeweight, "SHIELDMOD4" => $rankfourweight, "SHIELDMOD5" => $rankfiveweight,
        "RADIOACTIVEMOD" => $rankzeroweight, "RADIOACTIVEMOD1" => $rankoneweight, "RADIOACTIVEMOD2" => $ranktwoweight, "RADIOACTIVEMOD3" => $rankthreeweight, "RADIOACTIVEMOD4" => $rankfourweight, "RADIOACTIVEMOD5" => $rankfiveweight,
        "CARNAGEMOD" => $rankzeroweight, "CARNAGEMOD1" => $rankoneweight, "CARNAGEMOD2" => $ranktwoweight, "CARNAGEMOD3" => $rankthreeweight, "CARNAGEMOD4" => $rankfourweight, "CARNAGEMOD5" => $rankfiveweight,
        "PRINTMOD" => $rankzeroweight, "PRINTMOD1" => $rankoneweight, "PRINTMOD2" => $ranktwoweight, "PRINTMOD3" => $rankthreeweight, "PRINTMOD4" => $rankfourweight, "PRINTMOD5" => $rankfiveweight,
        "SLEDGEDASH" => $rankzeroweight, "SLEDGEDASH1" => $rankoneweight, "SLEDGEDASH2" => $ranktwoweight, "SLEDGEDASH3" => $rankthreeweight, "SLEDGEDASH4" => $rankfourweight, "SLEDGEDASH5" => $rankfiveweight,
        "DASHMOD" => $rankzeroweight, "DASHMOD1" => $rankoneweight, "DASHMOD2" => $ranktwoweight, "DASHMOD3" => $rankthreeweight, "DASHMOD4" => $rankfourweight, "DASHMOD5" => $rankfiveweight,
        "STAMINAMOD" => $rankzeroweight, "STAMINAMOD1" => $rankoneweight, "STAMINAMOD2" => $ranktwoweight, "STAMINAMOD3" => $rankthreeweight, "STAMINAMOD4" => $rankfourweight, "STAMINAMOD5" => $rankfiveweight
    );

    if (isset($weaponRanks[$item_name_without_numbers])) {
        return $weaponRanks[$item_name_without_numbers] * getWeaponUpgradeMultiplier($item_name);
    } elseif (isset($modRanks[$item_name])) {
        return $modRanks[$item_name];
    } else {
        return 0; // Default rank for unknown items
    }


}




function getModType($item_name) {
    // You can adjust the mod types and their corresponding items
    $modTypes = array(
        "DAMAGEMOD" => array("DAMAGEMOD", "DAMAGEMOD1", "DAMAGEMOD2", "DAMAGEMOD3", "DAMAGEMOD4", "DAMAGEMOD5"),
        "BLASTMOD" => array("BLASTMOD", "BLASTMOD1", "BLASTMOD2", "BLASTMOD3", "BLASTMOD4", "BLASTMOD5"),
        "JUMPMOD" => array("JUMPMOD", "JUMPMOD1", "JUMPMOD2", "JUMPMOD3", "JUMPMOD4", "JUMPMOD5"),
        "MAXHPMOD" => array("MAXHPMOD", "MAXHPMOD1", "MAXHPMOD2", "MAXHPMOD3", "MAXHPMOD4", "MAXHPMOD5"),
        "SPEEDMOD" => array("SPEEDMOD", "SPEEDMOD1", "SPEEDMOD2", "SPEEDMOD3", "SPEEDMOD4", "SPEEDMOD5"),
        "REGENMOD" => array("REGENMOD", "REGENMOD1", "REGENMOD2", "REGENMOD3", "REGENMOD4", "REGENMOD5"),
        "SIPHONMOD" => array("SIPHONMOD", "SIPHONMOD1", "SIPHONMOD2", "SIPHONMOD3", "SIPHONMOD4", "SIPHONMOD5"),
        "LEECHMOD" => array("LEECHMOD", "LEECHMOD1", "LEECHMOD2", "LEECHMOD3", "LEECHMOD4", "LEECHMOD5"),
        "SHIELDMOD" => array("SHIELDMOD", "SHIELDMOD1", "SHIELDMOD2", "SHIELDMOD3", "SHIELDMOD4", "SHIELDMOD5"),
        "RADIOACTIVEMOD" => array("RADIOACTIVEMOD", "RADIOACTIVEMOD1", "RADIOACTIVEMOD2", "RADIOACTIVEMOD3", "RADIOACTIVEMOD4", "RADIOACTIVEMOD5"),
        "CARNAGEMOD" => array("CARNAGEMOD", "CARNAGEMOD1", "CARNAGEMOD2", "CARNAGEMOD3", "CARNAGEMOD4", "CARNAGEMOD5"),
        "PRINTMOD" => array("PRINTMOD", "PRINTMOD1", "PRINTMOD2", "PRINTMOD3", "PRINTMOD4", "PRINTMOD5"),
        "SLEDGEDASH" => array("SLEDGEDASH", "SLEDGEDASH1", "SLEDGEDASH2", "SLEDGEDASH3", "SLEDGEDASH4", "SLEDGEDASH5"),
        "DASHMOD" => array("DASHMOD", "DASHMOD1", "DASHMOD2", "DASHMOD3", "DASHMOD4", "DASHMOD5"),
        "STAMINAMOD" => array("STAMINAMOD", "STAMINAMOD1", "STAMINAMOD2", "STAMINAMOD3", "STAMINAMOD4", "STAMINAMOD5")
    );

    foreach ($modTypes as $modType => $items) {
        if (in_array($item_name, $items)) {
            return $modType;
        }
    }

    return null; // Return null if the item is not a mod or not found in the mod types
}


function getWeaponRank($item_name) {

    $item_name = str_replace("M416", "X", $item_name);

    // Remove trailing numbers from the item name
    $item_name_without_numbers = preg_replace('/\d+$/', '', $item_name);



    // Example rank values for weapons (you can adjust these values)
    $weaponRanks = array(
        "QUADROCKET" => 19,
        "ROCKETER" => 18,
        "TRIPLESHOT" => 17,
        "DOUBLEBALL" => 16,
        "SHOTGUN" => 15,
        "PLASMAGUN" => 14,
        "LASERLANCE" => 13,
        "SINGLEBALL" => 12,
        "RAILER" => 11,
        "M416" => 10,
        "REDLASER" => 9,
        "BLUELASER" => 8,
        "SMG" => 7,
        "GRAVITYGUN" => 6,
        "BUZZSAW" => 5,
        "LIGHTNINGER" => 4,
        "SNIPER" => 3,
        "SLEDGER" => 2,
        "BLASTER" => 1
    );

    // Check if the provided item name exists in the weapon ranks array
    if (array_key_exists($item_name_without_numbers, $weaponRanks)) {
        return $weaponRanks[$item_name_without_numbers] * getWeaponUpgradeMultiplier($item_name); // Return the rank of the item
    } else {
        return 0; // Return 0 if the item name is not found
    }
}



function calculateGearscore2($player_id, $pdo) {

$gearscore = 0;

$sql = "SELECT item_id, quantity FROM player_items WHERE player_id = :player_id";
$stmt = $pdo->prepare($sql);
$stmt->execute(['player_id' => $player_id]);


  // Initialize the gearscore and the best mods and weapons arrays
  $gearscore = 0;
  $best_mods = [];
  $best_weapons = [];


    // Loop through each item in the inventory
    while ($row = $stmt->fetch(PDO::FETCH_ASSOC)) {
        $item_id = $row['item_id'];
        $quantity = $row['quantity'];

        // Fetch the item name from the item_names table
        $sql_item = "SELECT item_name FROM item WHERE id = :item_id";
        $stmt_item = $pdo->prepare($sql_item);
        $stmt_item->execute(['item_id' => $item_id]);
        $item_name = $stmt_item->fetchColumn();

 
        // Check if the item is a mod or a weapon and update the best_mods and best_weapons arrays
        // You can create your own function to get the item type and rank based on the item_name
        $item_type = getItemType($item_name); // Returns "mod" or "weapon"
        $item_rank = getItemRank($item_name); // Returns a rank value based on the item's strength

        if ($quantity == 0) {
        $item_rank = 0;
        }
        if ($item_type == "mod") {
            $mod_type = getModType($item_name); // Returns the mod type (e.g., "BLASTMOD")
            if (!isset($best_mods[$mod_type]) || $item_rank > $best_mods[$mod_type]) {
                $best_mods[$mod_type] = $item_rank;
            }
        } elseif ($item_type == "weapon") {
            if (!in_array($item_name, $best_weapons)) {
                $best_weapons[] = $item_name;
            }
        }
    }

    // Calculate the Gearscore based on the best_mods and best_weapons arrays
    foreach ($best_mods as $mod_type => $mod_rank) {
        $gearscore += $mod_rank * 50; // MOD_RANK_FACTOR is a constant you define
    }
    foreach ($best_weapons as $weapon_name) {
        $weapon_rank = getWeaponRank($weapon_name); // Returns a rank value based on the weapon's strength
        $gearscore += $weapon_rank * 7; // WEAPON_RANK_FACTOR is a constant you define
    }

    return $gearscore;
}
const SUCCESS_CODE = '0';
const MISSING_POST_VAR_CODE = '2';
const PLAYER_NOT_FOUND_CODE = '3';
const UNKNOWN_ERROR_CODE = '7';

if (!isset($_POST["code1234"])) {
    echo MISSING_POST_VAR_CODE;
    exit();
}

$unity = true;
$login = true;
require_once '../../keys/storage.php';

try {
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

    $sql = "SELECT id FROM players";
    $stmt = $pdo->prepare($sql);
    $stmt->execute();

    $player_scores = [];
    $player_id = null;
    $player_gearscore = null;

    while ($row = $stmt->fetch(PDO::FETCH_ASSOC)) {
        $player_id_iter = $row['id'];
        $player_scores[$player_id_iter] = calculateGearscore2($player_id_iter, $pdo);
    }

    if (isset($_POST["player_name"])) {
        $player_name = $_POST["player_name"];
        $stmt = $pdo->prepare("SELECT id FROM players WHERE username = :username");
        $stmt->bindParam(':username', $player_name, PDO::PARAM_STR);
        $stmt->execute();

        $player_id = $stmt->fetchColumn();
        if (!$player_id) {
            echo PLAYER_NOT_FOUND_CODE;
            exit();
        }

        $player_gearscore = calculateGearscore2($player_id, $pdo);
        $player_scores[$player_id] = $player_gearscore;
    }

    arsort($player_scores);

    $top_players = array_slice($player_scores, 0, 10, true);
    $returnstring = SUCCESS_CODE;

    foreach ($top_players as $player_id_iter => $gearscore) {
        $stmt = $pdo->prepare("SELECT username FROM players WHERE id = :player_id");
        $stmt->bindParam(':player_id', $player_id_iter, PDO::PARAM_INT);
        $stmt->execute();

        $username = $stmt->fetchColumn();
        $returnstring .= "\t" . $player_id_iter . "\t" . $username . "\t" . $gearscore;
    }

    $player_rank = 1;
    foreach ($player_scores as $gearscore) {
        if ($gearscore > $player_gearscore) {
            $player_rank++;
        }
    }

    $returnstring .= "\t" . $player_rank . "\t" . $player_name . "\t" . $player_gearscore;

    echo $returnstring;
    exit();

} catch (PDOException $e) {
    error_log("Database error: " . $e->getMessage());
    echo UNKNOWN_ERROR_CODE;
    exit();
}
?>
