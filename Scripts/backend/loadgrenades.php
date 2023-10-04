<?php // loadgrenades.php


$unity = true; // required to interact with storage.php
$login = true; // required to interact with storage.php

if (!isset($_POST["action"])) {
    error_log("ISSUE WITH ITEM_NAME POST");
    exit();
}
$itemname = $_POST["action"];

require_once '../../keys/storage.php'; // grants pdo and conn

    // Check connection
    if ($conn->connect_error) {
        die("Connection failed: " . $conn->connect_error);
    }

    // Get the posted data
    $action = $_POST["action"];
    $playerUsername = $_POST["username"];
    $grenadeCountData = $_POST["grenade_count"];

    // Get player ID from the players table
    $sql = "SELECT id FROM players WHERE username = '$playerUsername'";
    $result = $conn->query($sql);
    $playerId = $result->fetch_assoc()["id"];

    // Check if the player has an entry in the player_stats table
    $sql = "SELECT COUNT(*) as cnt FROM player_stats WHERE player_id = $playerId";
    $result = $conn->query($sql);
    $count = $result->fetch_assoc()["cnt"];

    if ($action == "save") {
        if ($count > 0) {
            // Update the existing entry using prepared statement
            $sql = "UPDATE player_stats SET grenade_count = ? WHERE player_id = ?";
            $stmt = $conn->prepare($sql);
            $stmt->bind_param("si", $grenadeCountData, $playerId);
    
            if ($stmt->execute() === TRUE) {
                echo "grenade_count data saved";
            } else {
                echo "Error: " . $sql . "<br>" . $conn->error;
            }
    
        } else {
            // Create a new entry using prepared statement
            $sql = "INSERT INTO player_stats (player_id, grenade_count) VALUES (?, ?)";
            $stmt = $conn->prepare($sql);
            $stmt->bind_param("is", $playerId, $grenadeCountData);
    
            if ($stmt->execute() === TRUE) {
                echo "grenade_count data saved";
            } else {
                echo "Error: " . $sql . "<br>" . $conn->error;
            }
    
            $stmt->close();
        }
    } elseif ($action == "load") {
        if ($count > 0) {
            // Get the grenade_count data
            $sql = "SELECT grenade_count FROM player_stats WHERE player_id = $playerId";
            $result = $conn->query($sql);
            $grenadeCountData = $result->fetch_assoc()["grenade_count"];
            echo $grenadeCountData;
        } else {
            echo "No grenade data found";
        }


    } 

    $conn->close();

?>