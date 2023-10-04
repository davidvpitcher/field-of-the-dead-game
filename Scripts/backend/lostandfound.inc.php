<?php // lostandfound.inc.php

// Constants for response codes
const MISSING_POST_DATA = "Error: Missing username or itemName";
const PLAYER_NOT_FOUND = "Error: Player not found.";
const EXECUTION_FAILED = "Error: Execution failed.";

$unity = true;
$login = true;
require_once '../../keys/storage.php';

if (isset($_POST["username"]) && isset($_POST["itemName"])) {
    try {
        $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
        $pdo->beginTransaction();

        $username = $_POST["username"];
        $itemName = $_POST["itemName"];

        // Get player's ID
        $stmt = $pdo->prepare("SELECT id FROM players WHERE username = :username");
        $stmt->bindParam(':username', $username, PDO::PARAM_STR);
        $stmt->execute();
        $player_id = $stmt->fetchColumn();

        if (!$player_id) {
            echo PLAYER_NOT_FOUND;
            exit();
        }

        // Check if the item exists in the lostandfound table
        $stmt = $pdo->prepare("SELECT amount FROM lostandfound WHERE name = :name");
        $stmt->bindParam(':name', $itemName, PDO::PARAM_STR);
        $stmt->execute();
        $itemAmount = $stmt->fetchColumn();

        if ($itemAmount !== false) {
            // Update the item's amount
            $stmt = $pdo->prepare("UPDATE lostandfound SET amount = :amount WHERE name = :name");
            $itemAmount++;
            $stmt->bindParam(':amount', $itemAmount, PDO::PARAM_INT);
            $stmt->bindParam(':name', $itemName, PDO::PARAM_STR);
            $success = $stmt->execute();
        } else {
            // Insert new row for the item
            $stmt = $pdo->prepare("INSERT INTO lostandfound (name, amount) VALUES (:name, 1)");
            $stmt->bindParam(':name', $itemName, PDO::PARAM_STR);
            $success = $stmt->execute();
        }

        if ($success) {
            $pdo->commit();
            echo "0";  // Success code
        } else {
            $pdo->rollBack();
            echo EXECUTION_FAILED;
        }

    } catch (PDOException $e) {
        $pdo->rollBack();
        error_log("Database error: " . $e->getMessage());
        echo EXECUTION_FAILED;
        exit();
    }
} else {
    echo MISSING_POST_DATA;
    exit();
}
