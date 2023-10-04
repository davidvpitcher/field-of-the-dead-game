<?php // check_rewards.php
if (isset($_POST["username"])) {
    $unity = true;
    $login = true;
    $username = $_POST["username"];
    
    require_once '../../keys/storage.php';  // Assuming PDO is initialized here as $pdo

    // Transaction begins
    $pdo->beginTransaction();

    try {
        // Get the player's ID
        $sql = "SELECT id FROM players WHERE username = :username";
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':username', $username);
        $stmt->execute();
        $player_id = $stmt->fetchColumn();

        if (!$player_id) {
            throw new Exception("Error: Player not found.");
        }

        // Get the player's reward crate count
        $sql = "SELECT reward_crates FROM player_stats WHERE player_id = :player_id";
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':player_id', $player_id);
        $stmt->execute();
        $reward_crates = $stmt->fetchColumn();

        if ($reward_crates === false) {
            echo "0";
        } else {
            if ($reward_crates > 0) {
                // Subtract one reward crate
                $sql = "UPDATE player_stats SET reward_crates = reward_crates - 1 WHERE player_id = :player_id";
                $stmt = $pdo->prepare($sql);
                $stmt->bindValue(':player_id', $player_id);
                $stmt->execute();

                // Commit transaction
                $pdo->commit();
                echo $reward_crates;
            } else {
                echo $reward_crates;
            }
        }
    } catch (Exception $e) {
        // Rollback transaction if any exception occurs
        $pdo->rollBack();
        echo $e->getMessage();
    }
} else {
    echo "Error: Missing username";
}
