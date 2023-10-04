<?php // restoreobject.php

if (isset($_POST["BASICCHECK"])) {
    $unity = true;
    $login = true;
    require_once '../../keys/storage.php';

    try {
        $sql1 = 'SELECT * FROM `the_builded`';
        $stmt = $pdo->prepare($sql1);
        $stmt->execute();
        $totals = $stmt->rowCount();
        
        echo "0\t" . $totals;
    } catch (Exception $e) {
        echo "SERVER_ERROR: " . $e->getMessage();
    }
    
    exit();
} else {
    echo "7: uh oh";
    exit();
}
?>
