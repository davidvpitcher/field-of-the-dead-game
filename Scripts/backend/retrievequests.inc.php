<?php // retrievequests.inc.php

if (isset($_POST["username"])) {
    $unity = true;
    $login = true;
    
    $player_name = $_POST["username"];
    
    require_once '../../keys/storage.php'; // Assuming mysqli connection is initialized here as $conn

    $finalquestdata = "";
    $finalVar1 = "";
    $finalVar2 = "";
    $finalVar3 = "";
    $finalVar4 = "";

    try {
        // Get the player's ID
        $stmt = $conn->prepare("SELECT `id` FROM `players` WHERE `username` = ?");
        $stmt->bind_param("s", $player_name);
        $stmt->execute();
        $result = $stmt->get_result();
        $player_id = $result->fetch_assoc()["id"];

        if (!$player_id) {
            echo "5";
            exit();
        }

        // Retrieve player quests
        $stmt = $conn->prepare("SELECT `questdata`,`enemiesKilled`,`thingsDestroyed`,`grindCompleted`,`repeatProgress` FROM `player_quests` WHERE `player_id` = ?");
        $stmt->bind_param("i", $player_id);
        $stmt->execute();
        $result = $stmt->get_result();

        if ($result->num_rows == 0) {
            echo "5";
            exit();
        } else {
            while ($row = $result->fetch_assoc()) {
                $finalquestdata = $row["questdata"];
                $finalVar1 = $row["enemiesKilled"];
                $finalVar2 = $row["thingsDestroyed"];
                $finalVar3 = $row["grindCompleted"];
                $finalVar4 = $row["repeatProgress"];
            }
        }

        echo "0\t" . $finalquestdata . "\t" . $finalVar1 . "\t" . $finalVar2 . "\t" . $finalVar3 . "\t" . $finalVar4;
        exit();
    } catch (Exception $e) {
        echo "7: " . $e->getMessage();
        exit();
    }
} else {
    echo "7: uh oh";
    exit();
}
