<?php // upgradeitem.php

function getBaseItemNameAndLevel($itemName) {
    if (strpos($itemName, 'M416') === 0) {
        $baseItemName = 'M416';
        $levelString = str_replace('M416', '', $itemName);
        $currentLevel = $levelString === '' ? 0 : (int)$levelString;
    } else {
        $baseItemName = preg_replace('/\d+$/', '', $itemName);
        if (preg_match('/(\d+)$/', $itemName, $matches)) {
            $currentLevel = (int)$matches[1];
        } else {
            $currentLevel = 0;
        }
    }

    return [$baseItemName, $currentLevel];
}


if (isset($_POST["username"]) && isset($_POST["item_name"])) {
   


    $unity = true;
    $login = true;
    require_once '../../keys/storage.php';
    
 // Data Sanitization and Validation
 $username = filter_var($_POST["username"], FILTER_SANITIZE_STRING);
 $itemName = filter_var($_POST["item_name"], FILTER_SANITIZE_STRING);
 $usingStabilizers = filter_var($_POST["using_stabilizers"], FILTER_VALIDATE_INT);
 $requiredStabilizers = filter_var($_POST["required_stabilizers"], FILTER_VALIDATE_INT);
 $isGuaranteed = filter_var($_POST["is_guaranteed"], FILTER_VALIDATE_BOOLEAN);
 $isMod = filter_var($_POST["is_mod"], FILTER_VALIDATE_BOOLEAN);


    $checkstabilizers = 2;

    if ($usingStabilizers === "0") {
        $checkstabilizers = 0;
    }
    if ($usingStabilizers === "1") {
        $checkstabilizers = 1;
    }
    
    $getPlayerIdQuery = $pdo->prepare("SELECT `id` FROM `players` WHERE `username` = :username");
    $getPlayerIdQuery->bindValue(':username', $username);
    $getPlayerIdQuery->execute();
    $playerIdData = $getPlayerIdQuery->fetch(PDO::FETCH_ASSOC);
    $playerId = $playerIdData['id'];



    $getStabilizersQuery = $pdo->prepare("SELECT `quantity` FROM `player_items` WHERE `player_id` = :player_id AND `item_id` = (SELECT `id` FROM `item` WHERE `item_name` = 'STABILIZERS')");
$getStabilizersQuery->bindValue(':player_id', $playerId);
$getStabilizersQuery->execute();
$stabilizersData = $getStabilizersQuery->fetch(PDO::FETCH_ASSOC);
$playerStabilizers = $stabilizersData ? $stabilizersData['quantity'] : 0;

if ($usingStabilizers && $playerStabilizers < $requiredStabilizers) {
    echo "7: Not enough stabilizers";
    exit();
}


if ($isGuaranteed) {


    $fissionquery = "SELECT `quantity` FROM `player_items` WHERE `player_id` = :player_id AND `item_id` = (SELECT `id` FROM `item` WHERE `item_name` = 'FISSIONCATALYST')";
    $spacemassquery = "SELECT `quantity` FROM `player_items` WHERE `player_id` = :player_id AND `item_id` = (SELECT `id` FROM `item` WHERE `item_name` = 'SPACEMASS')";

    $mainquery = $spacemassquery;
    if ($isMod) {

        $mainquery = $fissionquery;
    }

    $mainquerybegin = $pdo->prepare($mainquery);

    $mainquerybegin->bindValue(':player_id', $playerId);
    $mainquerybegin->execute();
    $guaranteedata = $mainquerybegin->fetch(PDO::FETCH_ASSOC);
    $playerRiskRemovalItems = $guaranteedata ? $guaranteedata['quantity'] : 0;
    
    error_log("guaranteedata: ".$guaranteedata['quantity']);
    error_log("UPGRADEITEM: ".$playerRiskRemovalItems);
    if ($playerRiskRemovalItems < 1) {
        echo "9: Not enough risk removallers";
        exit();
    }
    

}

    $sql = 'SELECT pi.player_id, pi.quantity FROM players p JOIN player_items pi ON p.id = pi.player_id JOIN item i ON pi.item_id = i.id WHERE p.username = :username AND i.item_name = :item_name';
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':username', $username);
    $stmt->bindValue(':item_name', $itemName);

    
    $stmt->execute();

    $row = $stmt->fetch(PDO::FETCH_ASSOC);

    
    if ($row) {
        $playerItemId = $row['item_id'];
        $itemQuantity = $row['quantity'];


        if ($itemQuantity > 0) {
            $chance = mt_rand(1, 100);
            $upgradeSucceeded = false;

           list($baseItemName, $currentLevel) = getBaseItemNameAndLevel($itemName);
       

            $chances = [70, 50, 30, 15, 5, 1];
            if ($chance <= $chances[$currentLevel]) {
                $upgradeSucceeded = true;
            }
            $newItemName = $baseItemName  . ($currentLevel < 5 ? ($currentLevel + 1) : ($baseItemName == 'M416' ? '' : '0'));
            $downgradedItemName = $baseItemName  . ($currentLevel > 0 ? ($currentLevel - 1) : ($baseItemName == 'M416' ? '' : '0'));
            
            if (!$upgradeSucceeded && $usingStabilizers) {
                $newItemName = $itemName;
            }

            if ($isGuaranteed) {
                $upgradeSucceeded = true;
            }

            if ($upgradeSucceeded) {
                echo "0," . $newItemName . "," . $itemName.  "," .($usingStabilizers ? "1" : "0"). "," .($isGuaranteed ? "1" : "0"). "," .($isMod ? "1" : "0"); // Upgrade successful
            } else {
                echo "1," . ($usingStabilizers ? $itemName : $itemName) . "," . $itemName . "," . ($usingStabilizers ? "1" : "0"). "," .($isGuaranteed ? "1" : "0"). "," .($isMod ? "1" : "0"); // Upgrade failed
            }
            
        } else {
            echo "7"; // Item not in inventory
        }
    } else {
        echo "7"; // Item not in inventory
    }
} else {
    echo "7"; // Invalid request
}
