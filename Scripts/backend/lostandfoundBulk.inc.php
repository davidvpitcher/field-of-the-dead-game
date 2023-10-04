<?php // lostandfoundBulk.inc.php
if (isset($_POST["username"]) && isset($_POST["items"])) {
    $username = $_POST["username"];
    $items = json_decode($_POST["items"], true);

    if (json_last_error() !== JSON_ERROR_NONE) {
        echo "Error: Invalid JSON in items.";
        exit();
    }

    if (!isset($items['itemNames'])) {
        echo "Error: No items provided.";
        exit();
    }

    $unity = true;
    $login = true;
    require_once '../../keys/storage.php';



   try {
        $pdo->beginTransaction();

        // Get the player's ID
        $sql = "SELECT id FROM players WHERE username = :username";
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':username', $username);
        $stmt->execute();
        $player_id = $stmt->fetchColumn();

        if (!$player_id) {
            echo "Error: Player not found.";
            exit();
        }

        foreach ($items['itemNames'] as $itemName) {
        // Check if the item exists in the lostandfound table
        $sql = "SELECT amount FROM lostandfound WHERE name = :name";
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':name', $itemName);
        $stmt->execute();
        $itemAmount = $stmt->fetchColumn();

        if ($itemAmount !== false) {
            // Item exists, increment its amount
            $sql = "UPDATE lostandfound SET amount = :amount WHERE name = :name";
            $stmt = $pdo->prepare($sql);
            $stmt->bindValue(':name', $itemName);
            $stmt->bindValue(':amount', $itemAmount + 1);
            $success = $stmt->execute();
        } else {
            // Item does not exist, insert a new row
            $sql = "INSERT INTO lostandfound (name, amount) VALUES (:name, 1)";
            $stmt = $pdo->prepare($sql);
            $stmt->bindValue(':name', $itemName);
            $success = $stmt->execute();
        }

        if (!$success) {
            $pdo->rollBack();
            echo "Error: Execution failed for item: " . $itemName;
            exit();
        }
    }

    $pdo->commit();
    echo "0"; // Success code
} catch (PDOException $e) {
    $pdo->rollBack();
    echo "Error: Transaction failed. " . $e->getMessage();
    exit();
}

} else {
echo "Error: Missing username or items";
}
?>