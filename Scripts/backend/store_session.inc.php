<?php // store_session.inc.php

if (isset($_POST["action"]) && $_POST["action"] === "storeSession" && isset($_POST["username"]) && isset($_POST["sessionID"])) {
    
    $unity = true; // Required to access the keys
    $login = true; // Required to access the keys
    require_once '../../keys/storage.php'; // Grants pdo and conn

    try {
        $player_name = $_POST["username"];
        $sessionID = $_POST["sessionID"];

        // Prepare SQL statement to fetch player ID
        $stmt = $conn->prepare("SELECT `id` FROM `players` WHERE `username` = ?");
        $stmt->bind_param("s", $player_name);
        $stmt->execute();
        $result = $stmt->get_result();
        $player_id = $result->fetch_assoc()["id"];
        
        if (!$player_id) {
            echo "PLAYER_NOT_FOUND";
            exit();
        }

        // Prepare SQL statement to deactivate existing sessions for the user
        $stmt = $conn->prepare("UPDATE PlayerSessions SET IsActive=false WHERE PlayerId=?");
        $stmt->bind_param("i", $player_id);
        $stmt->execute();

        // Prepare SQL statement to insert new session information
        $stmt = $conn->prepare("INSERT INTO PlayerSessions (SessionID, PlayerId, PlayerName, IsActive) VALUES (?, ?, ?, true)");
        $stmt->bind_param("sis", $sessionID, $player_id, $player_name);
        $stmt->execute();

        echo "SESSION_STORED";
        mysqli_close($conn);
        exit();
    } catch (Exception $e) {
        echo "SERVER_ERROR: " . $e->getMessage();
        exit();
    }

} else {
    echo "INVALID_REQUEST";
    exit();
}
