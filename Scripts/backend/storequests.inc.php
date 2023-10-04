<?php // storequests.inc.php

if (isset($_POST["username"])) {
    $unity = true;
    $login = true;
    
    $player_name = $_POST["username"];
    $quest_data = $_POST["questdata"];
    $enemiesKilled = $_POST["enemiesKilled"];
    $thingsDestroyed = $_POST["thingsDestroyed"];
    $grindCompleted = $_POST["grindCompleted"];
    $repeatProgress = $_POST["repeatProgress"];
    
    require_once '../../keys/storage.php'; // Assuming mysqli connection is initialized here as $conn

    try {
        // Get the player's ID
        $stmt = $conn->prepare("SELECT `id` FROM `players` WHERE `username` = ?");
        $stmt->bind_param("s", $player_name);
        $stmt->execute();
        $result = $stmt->get_result();
        $player_id = $result->fetch_assoc()["id"];

        if (!$player_id) {
            echo "7: Player not found";
            exit();
        }




// Initialize variables
$current_highest_completed = "";

$stmt = $conn->prepare("SELECT highest_completed_questdata FROM player_quests WHERE player_id = ?");
$stmt->bind_param("i", $player_id);
$stmt->execute();
$result = $stmt->get_result();

// Check if the query returned any rows
if ($result->num_rows > 0) {
    $row = $result->fetch_assoc();
    $current_highest_completed = $row["highest_completed_questdata"];
}


// Check for NULL or empty string and set default value if needed
$current_highest_completed = $current_highest_completed ?? "";

// Calculate the total quest completion value for both new and current data
$new_total = array_sum(str_split($quest_data));
$current_total = array_sum(str_split($current_highest_completed));

// Determine which quest data string to save as highest_completed_questdata
$highest_questdata_to_save = ($new_total > $current_total) ? $quest_data : $current_highest_completed;




 $stmt = $conn->prepare("SELECT * FROM player_quests WHERE player_id = ?");
        $stmt->bind_param("i", $player_id);
        $stmt->execute();
        $result = $stmt->get_result();

        if ($result->num_rows == 0) {
            // Insert
            $stmt = $conn->prepare("INSERT INTO player_quests (player_id, questdata, highest_completed_questdata, enemiesKilled, thingsDestroyed, grindCompleted, repeatProgress) VALUES (?, ?, ?, ?, ?, ?, ?)");
            $stmt->bind_param("issssss", $player_id, $quest_data, $highest_questdata_to_save, $enemiesKilled, $thingsDestroyed, $grindCompleted, $repeatProgress);
            $stmt->execute();
        } else {
            // Update
            $stmt = $conn->prepare("UPDATE player_quests SET questdata = ?, highest_completed_questdata = ?, enemiesKilled = ?, thingsDestroyed = ?, grindCompleted = ?, repeatProgress = ? WHERE player_id = ?");
            $stmt->bind_param("ssssssi", $quest_data, $highest_questdata_to_save, $enemiesKilled, $thingsDestroyed, $grindCompleted, $repeatProgress, $player_id);
            $stmt->execute();
        }

        echo "0";
        exit();
    } catch (Exception $e) {
        echo "7: " . $e->getMessage();
        exit();
    }
} else {
    echo "7: uh oh";
    exit();
}