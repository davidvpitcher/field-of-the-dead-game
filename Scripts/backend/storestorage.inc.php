<?php // storestorage.inc.php

const ERROR_MISSING_PLAYERNAME = "1";
const ERROR_EXECUTION_FAILED = "2";

if (isset($_POST["playerName"]) && isset($_POST["storageData"])) {
    try {
        $unity = true;
        $login = true;
        require_once '../../keys/storage.php';

        $playerName = $_POST["playerName"];
        $storageData = $_POST["storageData"];

        // Get player ID
        $stmt = $conn->prepare("SELECT `id` FROM `players` WHERE `username` = ?");
        $stmt->bind_param("s", $playerName);
        $stmt->execute();
        $result = $stmt->get_result();
        $player_id = mysqli_fetch_assoc($result)["id"];

        // Check if the player stats entry exists
        $stmt = $conn->prepare("SELECT * FROM player_stats WHERE player_id = ?");
        $stmt->bind_param("i", $player_id);
        $stmt->execute();
        $result = $stmt->get_result();
        
        if (mysqli_num_rows($result) == 0) {
            // If no entry exists, create one
            $stmt = $conn->prepare("INSERT INTO player_stats (player_id, storage_data) VALUES (?, ?)");
            $stmt->bind_param("is", $player_id, $storageData);
            $stmt->execute();
        } else {
            // If an entry exists, update it
            $stmt = $conn->prepare("UPDATE `player_stats` SET `storage_data` = ? WHERE `player_id` = ?");
            $stmt->bind_param("si", $storageData, $player_id);
            $stmt->execute();
        }

        mysqli_close($conn);
        echo "0";
        
    } catch (Exception $e) {
        error_log("Database error: " . $e->getMessage());
        echo ERROR_EXECUTION_FAILED;
    }

    exit();
} else {
    echo ERROR_MISSING_PLAYERNAME;
    exit();
}
