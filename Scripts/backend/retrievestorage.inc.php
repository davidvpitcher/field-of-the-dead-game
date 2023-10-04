<?php // retrievestorage.inc.php

const ERROR_MISSING_PLAYERNAME = "1";
const ERROR_STORAGE_NOT_FOUND = "2";
const ERROR_EXECUTION_FAILED = "3";

if (isset($_POST["playerName"])) {
    try {
        $unity = true;
        $login = true;
        require_once '../../keys/storage.php';

        $playerName = $_POST["playerName"];
        
        $stmt = $conn->prepare("SELECT `id` FROM `players` WHERE `username` = ?");
        $stmt->bind_param("s", $playerName);
        $stmt->execute();
        $result = $stmt->get_result();
        $player_id = mysqli_fetch_assoc($result)["id"];
        
        $stmt = $conn->prepare("SELECT `storage_data` FROM `player_stats` WHERE `player_id` = ?");
        $stmt->bind_param("i", $player_id);
        $stmt->execute();
        $result = $stmt->get_result();
        
        if (mysqli_num_rows($result) > 0) {
            $storageData = mysqli_fetch_assoc($result)["storage_data"];
            echo $storageData;
        } else {
            echo ERROR_STORAGE_NOT_FOUND;
        }
        
        mysqli_close($conn);
        
    } catch (Exception $e) {
        error_log("Database error: " . $e->getMessage());
        echo ERROR_EXECUTION_FAILED;
    }
    exit();
} else {
    echo ERROR_MISSING_PLAYERNAME;
    exit();
}
?>
