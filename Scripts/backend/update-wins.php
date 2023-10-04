<?php // update-wins.php

if (isset($_POST["username"])) {
    $unity = true;
    $login = true;
    $username = $_POST["username"];
    
    require_once '../../keys/storage.php';

    // Initialize response array
    $response = [];

    try {
        // Begin transaction
        $pdo->beginTransaction();

        // Get the player's ID
        $sql = "SELECT id FROM players WHERE username = :username";
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':username', $username);
        $stmt->execute();
        $player_id = $stmt->fetchColumn();

        if (!$player_id) {
            // Player not found, return error code
            echo "Error: Player not found.";
            exit();
        }

        // Check if the player's stats record exists
        $sql = "SELECT COUNT(*) FROM player_stats WHERE player_id = :player_id";
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':player_id', $player_id);
        $stmt->execute();
        $recordExists = $stmt->fetchColumn();
        
        if ($recordExists > 0) {
            // Update the record
            $sql = "UPDATE player_stats SET wins = wins + 1, reward_crates = reward_crates + 1 WHERE player_id = :player_id";
        } else {
            // Insert new record
            $sql = "INSERT INTO player_stats (player_id, wins, reward_crates) VALUES (:player_id, 1, 1)";
        }

        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':player_id', $player_id);
        $success = $stmt->execute();
        
        if ($success) {
            // Commit transaction
            $pdo->commit();
            echo "0"; // Success code
        } else {
            // Rollback transaction
            $pdo->rollBack();
            echo "Error: Execution failed.";
        }
    } catch (PDOException $e) {
        $pdo->rollBack();
        $response["status"] = "error";
        $response["message"] = "Database error: " . $e->getMessage();
        echo json_encode($response);
        exit();
    }
} else {
    echo "Error: Missing username";
}
?>
