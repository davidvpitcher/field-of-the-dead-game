<?php // update_chosen_class_statback.php
$unity = true;
$login = true;
require_once '../../keys/storage.php';

if (!isset($_POST["username"])) {
    echo "2"; // Missing POST variable
    exit();
}

// Sanitize and Validate Input
$username = trim($_POST["username"]);
$class_stat = 0; // hardcoded as per your original script

if (empty($username)) {
    echo "2"; // Invalid POST variable
    exit();
}

try {
    // Begin transaction
    $pdo->beginTransaction();

    $sql = "INSERT INTO player_stats (player_id, chosen_class) VALUES ((SELECT id FROM players WHERE username = :username), :class_stat) ON DUPLICATE KEY UPDATE chosen_class = :class_stat";
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':username', $username, PDO::PARAM_STR);
    $stmt->bindValue(':class_stat', $class_stat, PDO::PARAM_INT);

    if ($stmt->execute()) {
        $pdo->commit();
        echo "0"; // success
    } else {
        $pdo->rollBack();
        echo "1"; // error
    }
} catch (PDOException $e) {
    $pdo->rollBack();
    echo "Database error: " . $e->getMessage();
    exit();
}
?>
